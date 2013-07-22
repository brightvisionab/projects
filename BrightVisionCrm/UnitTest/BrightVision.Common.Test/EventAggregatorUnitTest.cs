using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BrightVision.Common.Events.Core;
using BrightVision.Common.Events;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
namespace BrightVision.Common.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class EventAggregatorUnitTest
    {
        public EventAggregatorUnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestEventAggregator()
        {
            //
            // TODO: Add test logic here
            //
            EventAggregator events = new EventAggregator();
            bool isTriggeredNormalSubscibe = false;
            bool isTriggeredConditionalSubscibe = false;
            bool isTriggeredConditionalNotSubscibe = false;
            events.GetEvent<TestEvent>().Subscribe(delegate(TestEvent q)
            {
                isTriggeredNormalSubscibe = true;
            });

            events.GetEvent<TestEvent>().Where(e => e.Val == "Hello").Subscribe(delegate(TestEvent q)
            {
                isTriggeredConditionalSubscibe = true;
                q.Val = "Hello 1";
            });

            events.GetEvent<TestEvent>().Where(e => e.Val == "Hi ").Subscribe(delegate(TestEvent q)
            {
                isTriggeredConditionalNotSubscibe = true;
            });
            events.GetEvent<TestEvent>().Where(e => e.Val == "Hi ").Subscribe(OnTestEvent);

            TestEvent newTestEvent = new TestEvent { Val = "Hello" };
            events.Notify<TestEvent>(newTestEvent);
            events.Notify<TestEvent>(new TestEvent { Val = "Hi" });
            events.Notify<TestEvent2>(new TestEvent2 { });

            Assert.IsTrue(isTriggeredNormalSubscibe, "Event is not triggered");
            Assert.IsTrue(isTriggeredConditionalSubscibe, "Event condition subscribe triggered");
            Assert.IsFalse(isTriggeredConditionalNotSubscibe, "Event condition not subscribe triggered");
        }
        public void OnTestEvent(TestEvent e)
        {
            Console.WriteLine("Event Invoked!");
        }
    }
}
