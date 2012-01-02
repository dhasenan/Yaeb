using System;
using System.Reflection;
using NUnit.Framework;
using Events;
using Castle.Windsor;
using Castle.Windsor.Configuration;
using Castle.MicroKernel.Registration;

namespace Events.Tests
{
	[TestFixture]
	public class EventBrokerTests
	{
		private EventBroker _target;
		
		[SetUp]
		public void Setup ()
		{
			_target = new EventBroker ();
		}
		
		[Test]
		public void PublishAnEventWithNoArgumentsToOneSubscriber ()
		{
			var pub = new Publisher ();
			var sub = new Subscriber ();
			_target.Register (sub);
			_target.Register (pub);
			Assert.That (sub.callCount, Is.EqualTo (0));
			pub.TriggerEvent ();
			Assert.That (sub.callCount, Is.EqualTo (1));
		}
		
		[Test]
		public void PublishAnEventWithNoArgumentsToTwoSubscribers ()
		{
			var pub = new Publisher ();
			var sub1 = new Subscriber ();
			var sub2 = new Subscriber ();
			_target.Register (sub1);
			_target.Register (sub2);
			_target.Register (pub);
			Assert.That (sub1.callCount, Is.EqualTo (0));
			Assert.That (sub2.callCount, Is.EqualTo (0));
			pub.TriggerEvent ();
			Assert.That (sub1.callCount, Is.EqualTo (1));
			Assert.That (sub2.callCount, Is.EqualTo (1));
		}
		
		[Test]
		public void LowLevelPublish ()
		{
			var pub = new Publisher ();
			var sub1 = new Subscriber ();
			var sub2 = new Subscriber ();
			_target.Register (sub1);
			_target.Register (sub2);
			_target.AddPublisher(new string[]{"event2"}, pub, pub.GetType().GetEvent("Eventy", BindingFlags.Public | BindingFlags.Instance));
			Assert.That (sub1.callCount2, Is.EqualTo (0));
			Assert.That (sub2.callCount2, Is.EqualTo (0));
			pub.TriggerEvent ();
			Assert.That (sub1.callCount2, Is.EqualTo (1));
			Assert.That (sub2.callCount2, Is.EqualTo (1));
		}
		
		[Test]
		public void LowLevelSubscription ()
		{
			var pub = new Publisher ();
			var sub1 = new Subscriber ();
			_target.AddSubscriber ("event1", sub1, sub1.GetType ().GetMethod ("EventHappened", BindingFlags.Public | BindingFlags.Instance));
			_target.Register (pub);
			Assert.That (sub1.callCount, Is.EqualTo (0));
			pub.TriggerEvent ();
			Assert.That (sub1.callCount, Is.EqualTo (1));
			Assert.That (sub1.callCount2, Is.EqualTo (0));
			Assert.That (sub1.callCount3, Is.EqualTo (0));
		}
		
		[Test]
		public void LowLevelSubscriptionIgnoresAttributes ()
		{
			var pub = new Publisher ();
			var sub1 = new Subscriber ();
			_target.AddSubscriber ("event1", sub1, sub1.GetType ().GetMethod ("Event2Happened", BindingFlags.Public | BindingFlags.Instance));
			_target.AddSubscriber ("event1", sub1, sub1.GetType ().GetMethod ("Event3Happened", BindingFlags.Public | BindingFlags.Instance));
			_target.Register (pub);
			Assert.That (sub1.callCount, Is.EqualTo (0));
			pub.TriggerEvent ();
			Assert.That (sub1.callCount, Is.EqualTo (0));
			Assert.That (sub1.callCount2, Is.EqualTo (1));
			Assert.That (sub1.callCount3, Is.EqualTo (1));
		}
		
		[Test]
		public void WindsorStuffs ()
		{
			WindsorContainer c = new WindsorContainer ();
			c.Register(Component.For<IEventBroker>().ImplementedBy<EventBroker>());
			c.AddFacility<EventBrokerFacility> ();
			c.Register (Component.For<Publisher> ().ImplementedBy<Publisher> ());
			c.Register (Component.For<Subscriber> ().ImplementedBy<Subscriber> ());
			var sub = c.Resolve<Subscriber> ();
			var pub = c.Resolve<Publisher> ();
			pub.TriggerEvent ();
			Assert.That (sub.callCount, Is.EqualTo (1));
		}
		
		public class Publisher
		{
			[Publish("event1")]
			public event Action Eventy;
			
			public void TriggerEvent ()
			{
				if (Eventy != null)
				{
					Eventy ();
				}
			}
		}
		
		public class Subscriber
		{
			public int callCount;
			public int callCount2;
			public int callCount3;
			
			[Subscribe("event1")]
			public void EventHappened ()
			{
				callCount++;
			}
			
			[Subscribe("event2")]
			public void Event2Happened ()
			{
				callCount2++;
			}
			
			public void Event3Happened ()
			{
				callCount3++;
			}
		}
	}
}

