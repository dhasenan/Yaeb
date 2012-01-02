Yaeb -- Yet Another Event Broker
================================

(for C# and .NET)

The idea behind Yaeb is simple: annotation-based event registration. It's intended to work with Castle, but it should function just fine on its own. At the moment, it only supports no-argument events, though I'm looking to extend it to do one-argument events.

For usage examples, look at EventBrokerTests.cs in the test project.

To use Yaeb with Castle, make sure that you register the EventBroker component first, and then add the EventBrokerFacility, and then go on about your business of registering and instantiating more components.

When dealing with Castle, we minimize the amount of reflection we do insofar as possible: only look through a type's members when the component is registered, cache that in Castle's extended metadata, and read through that when the component is instantiated. This should be reasonably fast. If you aren't using Castle, well, Yaeb doesn't contain its own metadata store, so it does the full reflection deal every time. I may change that if I need to.

Subscribers are held using weak references and cleaned up whenever we notice that there's stuff to clean up. So if you have components that only exist by merit of subscribing to and publishing events, you probably want to hold a reference to them elsewhere.

Good luck!
