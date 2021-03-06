﻿using EventCentric.Log;
using EventCentric.Publishing;
using EventCentric.Publishing.Dto;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;

namespace EventCentric.Messaging
{
    public class InMemoryEventPublisher : IInMemoryEventPublisher
    {
        private readonly ILogger log;
        private readonly ConcurrentDictionary<string, Func<long, string, PollResponse>> sourcesByStreamType = new ConcurrentDictionary<string, Func<long, string, PollResponse>>();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, IOcassionallyConnectedSourceConsumer>> ocassionallyConnectedSourcesByConsumer = new ConcurrentDictionary<string, ConcurrentDictionary<string, IOcassionallyConnectedSourceConsumer>>();

        public InMemoryEventPublisher(ILogger log)
        {
            Ensure.NotNull(log, nameof(log));

            this.log = log;
        }

        public void Register(IPollableEventSource publisher)
        {
            sourcesByStreamType.TryAdd(publisher.SourceName, (version, name) => publisher.PollEvents(version, name));
            if (publisher is IOcassionallyConnectedSourceConsumer)
            {
                var consumer = publisher as IOcassionallyConnectedSourceConsumer;

                this.ocassionallyConnectedSourcesByConsumer.TryAdd(consumer.ConsumerName, new ConcurrentDictionary<string, IOcassionallyConnectedSourceConsumer>());
                this.ocassionallyConnectedSourcesByConsumer[consumer.ConsumerName].TryAdd(consumer.SourceName, consumer);
            }
        }

        public PollResponse PollEvents(string streamType, long fromVersion, string consumerName)
        {
            return this.sourcesByStreamType[streamType].Invoke(fromVersion, consumerName);
        }

        public bool TryUpdateConsumer(string consumerName, PollResponse response, out ServerStatus status)
        {
            status = null;
            if (!this.ocassionallyConnectedSourcesByConsumer.ContainsKey(consumerName))
                return false;

            var sources = this.ocassionallyConnectedSourcesByConsumer[consumerName];
            if (!sources.ContainsKey(response.StreamType))
                return false;

            var consumer = sources[response.StreamType];
            lock (consumer)
            {
                status = consumer.UpdateConsumer(response);
            }
            return true;
        }
    }
}
