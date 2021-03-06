﻿using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Utils;
using System;
using System.Collections.Generic;

namespace PersistenceBenchmark
{
    public class UserManagementHandler : Handler<UserManagement>,
        IHandle<CreateOrUpdateUser>,
        IHandle<AddNewSubscription>
    //IHandles<FreePointsRewardedToUser>
    {
        public UserManagementHandler(IBus bus, ILogger log, IEventStore<UserManagement> store) : base(bus, log, store)
        {
            this.EnableDeduplicationBeforeHandling = false;
        }

        //public IMessageHandling Handle(FreePointsRewardedToUser e)
        //{
        //    return base.FromStream(e.UserId, s =>
        //    s.Update(new UserReceivedPoints(s.Id, e.Points)));
        //}

        public IMessageHandling Handle(CreateOrUpdateUser c) =>
            base.FromNewStreamIfNotExists(c.UserId, s =>
            s.Update(new UserCreatedOrUpdated(c.UserId, c.Name)));


        public IMessageHandling Handle(AddNewSubscription message)
        {
            return base.FromNewStream(SequentialGuid.New, state =>
            state.UpdateAfterSending(new TryAddNewSubscription(message.SubscriberStreamType, message.StreamTypeOfProducer, message.Url, message.Token)));
        }

        private List<CreateOrUpdateUser> CreateCreateUserCommands(int quantity, int waves)
        {
            var list = new List<CreateOrUpdateUser>(quantity);
            for (int i = 0; i < quantity; i++)
            {
                var id = SequentialGuid.New;

                for (int j = 0; j < waves; j++)
                {
                    list.Add(new CreateOrUpdateUser(id, $"User_{id.ToString()}"));
                }
            }
            return list;
        }

        public void StressWithWavesOfConcurrentUsers(int wavesCount, int concurrentUsers, bool sendNewSub = false)
        {
            Console.WriteLine($"System is receiving {wavesCount * concurrentUsers} messages.");
            var wavesOfCommands = new List<List<CreateOrUpdateUser>>();

            wavesOfCommands.Add(CreateCreateUserCommands(concurrentUsers, wavesCount));

            wavesOfCommands.ForEach(w =>
            {
                System.Threading.ThreadPool.UnsafeQueueUserWorkItem(_ => w.ForEach(c => this.Process(c)), null);
            });
            if (sendNewSub)
            {
                this.Process(new AddNewSubscription("promo", "user2", Constants.InMemorySusbscriptionUrl, ""));
            }
            Console.WriteLine($"System is now handling {wavesCount * concurrentUsers} messages.");
        }
    }
}
