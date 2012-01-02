using System;
using System.Collections.Generic;
using System.Reflection;

namespace Events
{
	public class EventBroker : IEventBroker
	{
		private struct Subscription
		{
			public MethodInfo Method;
			public WeakReference Target;
		}
		
		private class Publication
		{
			public string[] Topics;
			public EventBroker Broker;

			public void Fire ()
			{
				foreach (var topic in Topics)
				{
					Broker.Fire (topic);
				}
			}
		}
		
		private Dictionary<string, List<Subscription>> _subscriptions = new Dictionary<string, List<Subscription>> ();
		private readonly MethodInfo _pubmethod;
		
		public EventBroker ()
		{
			_pubmethod = typeof(Publication).GetMethod ("Fire");
		}
		
		public void AddSubscriber (string topic, object o, MethodInfo method)
		{
			if (o == null)
			{
				throw new ArgumentNullException ("o");
			}
			if (method == null)
			{
				throw new ArgumentNullException ("method");
			}
			List<Subscription > subs;
			if (!_subscriptions.TryGetValue (topic, out subs))
			{
				subs = _subscriptions [topic] = new List<Subscription> ();
			}
			subs.Add (new Subscription{Method = method, Target = new WeakReference (o)});
		}
		
		public void AddPublisher (string[] topics, object o, EventInfo evt)
		{
			if (o == null)
			{
				throw new ArgumentNullException ("o");
			}
			if (evt == null)
			{
				throw new ArgumentNullException ("evt");
			}
			var pub = new Publication {Topics = topics, Broker = this};
			var dg = Delegate.CreateDelegate (typeof(Action), pub, _pubmethod);
			evt.AddEventHandler (o, dg);
		}
		
		public void Register (object o)
		{
			if (o == null)
			{
				return;
			}
			var type = o.GetType ();
			foreach (var method in type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public))
			{
				var subscribings = method.GetCustomAttributes (typeof(SubscribeAttribute), true);
				foreach (SubscribeAttribute sub in subscribings)
				{
					AddSubscriber (sub.Topic, o, method);
				}
			}
			foreach (var evt in type.GetEvents(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public))
			{
				var publishings = evt.GetCustomAttributes (typeof(PublishAttribute), true);
				if (publishings.Length > 0)
				{
					var p = new string[publishings.Length];
					int i = 0;
					foreach (PublishAttribute attr in publishings)
					{
						p [i] = attr.Topic;
						i++;
					}
					AddPublisher (p, o, evt);
				}
			}
		}
		
		public void Fire (string name)
		{
			bool needToClean = false;
			List<Subscription > subs;
			if (_subscriptions.TryGetValue (name, out subs))
			{
				foreach (var sub in subs)
				{
					var target = sub.Target.Target;
					if (target == null)
					{
						needToClean = true;
					}
					else
					{
						sub.Method.Invoke (target, new object[0]);
					}
				}
			}
			if (needToClean)
			{
				Clean (subs);
			}
		}
		
		private void Clean (List<Subscription> subs)
		{
			int i = 0;
			while (i < subs.Count)
			{
				if (subs [i].Target.IsAlive)
				{
					i++;
				}
				else
				{
					// it's efficient to slice off the end of the list
					// but not to remove inside the list
					// so we bring in the last item to this location, don't advance
					// position, and slice off the end of the list
					subs [i] = subs [subs.Count - 1];
					subs.RemoveAt (subs.Count - 1);
				}
			}
		}
	}
}

