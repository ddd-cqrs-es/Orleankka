﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleankka;
using Orleans.Concurrency;

namespace Example.Azure
{
    [StatelessWorker, Orleankka.Interleave(typeof(Publish))]
    public class HubBuffer : Actor
    {
        [Serializable]
        public class Publish
        {
            public Event Event;
        }

        readonly TimeSpan flushPeriod = TimeSpan.FromSeconds(1);        
        readonly Queue<Event> buffer = new Queue<Event>();
        
        ActorRef hub;

        public override Task OnActivate()
        {
            hub = HubGateway.GetLocalHub();

            Timers.Register("flush", flushPeriod, flushPeriod, Flush);

            return base.OnActivate();
        }

        Task Flush()
        {
            if (buffer.Count == 0)
                return Task.CompletedTask;

            var events = buffer.ToArray();
            buffer.Clear();

            return hub.Tell(new Hub.Publish{Events = events});
        }

        public void Handle(Publish req)
        {
            buffer.Enqueue(req.Event);
        }
    }
}