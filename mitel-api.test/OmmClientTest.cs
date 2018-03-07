using System;
using System.Threading;
using System.Threading.Tasks;
using mitelapi;
using mitelapi.Events;
using mitelapi.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace mitel_api.test
{
    [TestClass]
    public class OmmClientTest
    {
        private OmmClient _client;

        [TestInitialize]
        public void Setup()
        {
            _client = new OmmClient("localhost");
        }

        [TestCleanup]
        public void TearDown()
        {
            _client.Dispose();
        }

        [TestMethod]
        public async Task CanLogin()
        {
            await _client.LoginAsync("omm", "omm");
        }

        [TestMethod]
        public async Task CanLoginAsMOM()
        {
            await _client.LoginAsync("omm", "omm", true);
        }

        [TestMethod]
        public async Task EmptyPasswordThrowsException()
        {
            try
            {
                await _client.LoginAsync("omm", String.Empty);
                Assert.Fail("expected exception not thrown");
            }
            catch (OmmException ex)
            {
                Assert.AreEqual(OmmError.EAuth, ex.ErrorCode);
            }
        }

        [TestMethod]
        public async Task InvalidPasswordThrowsException()
        {
            try
            {
                await _client.LoginAsync("omm", "invalid password");
                Assert.Fail("expected exception not thrown");
            }
            catch (OmmException ex)
            {
                Assert.AreEqual(OmmError.EAuth, ex.ErrorCode);
            }
        }

        [TestMethod]
        public async Task CanPing()
        {
            await CanLogin();
            await _client.Ping(CancellationToken.None);
            Console.WriteLine(_client.Rtt);
        }

        [TestMethod]
        public async Task CanSubscribe()
        {
            await CanLogin();
            await _client.Subscribe(EventType.DECTSubscriptionMode, CancellationToken.None);
            var resetEvent = new ManualResetEventSlim();
            _client.DECTSubscriptionModeChanged += (s, e) =>
            {
                var data = e.Event;
                Console.WriteLine($"{data.Mode}");
                resetEvent.Set();
            };
            resetEvent.Wait(TimeSpan.FromSeconds(5));
            Assert.IsTrue(resetEvent.IsSet);
        }

        [TestMethod]
        public async Task CanSubscribeAlarmCallProgress()
        {
            await CanLogin();
            await _client.Subscribe(new SubscribeCmd(EventType.AlarmCallProgress){Ppn = -1, Trigger = "*"}, CancellationToken.None);
            var resetEvent = new ManualResetEventSlim();
            _client.AlarmCallProgress += (s, e) =>
            {
                var data = e.Event;
                Console.WriteLine($"{data.Id}: {data.Ppn} - {data.Destination} ({data.State}) ({data.Trigger})");
                resetEvent.Set();
            };
            resetEvent.Wait();
            Assert.IsTrue(resetEvent.IsSet);
        }
    }
}
