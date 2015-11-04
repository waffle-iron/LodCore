﻿using System.Linq;
using DataAccess.Entities;
using Journalist;
using NHibernate.Criterion;
using NHibernate.Linq;
using NotificationService;

namespace DataAccess.Repositories
{
    public class EventRepository : IEventRepository
    {
        public EventRepository(DatabaseSessionProvider sessionProvider)
        {
            Require.NotNull(sessionProvider, nameof(sessionProvider));

            _sessionProvider = sessionProvider;
        }

        public void DistrubuteEvent(Event @event, DistributionPolicy distributionPolicy)
        {
            using (var session = _sessionProvider.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var eventId = (int) session.Save(@event);
                foreach (var receiverId in distributionPolicy.ReceiverIds)
                {
                    session.Save(new Delivery(receiverId, eventId));
                }

                transaction.Commit();
            }
        }

        public Event[] GetEventsByUser(int userId, bool newOnly)
        {
            using (var session = _sessionProvider.OpenSession())
            {
                var userDeliveries = session.Query<Delivery>().Where(delivery => delivery.UserId == userId);
                var eventIds = newOnly
                    ? userDeliveries.Where(delivery => !delivery.WasRead).Select(delivery => delivery.EventId)
                    : userDeliveries.Select(delivery => delivery.EventId);
                var events = session.Query<Event>().Where(@event => eventIds.Contains(@event.Id)).ToArray();
                return events;
            }
        }

        public void MarkEventsAsRead(int[] eventIds)
        {
            using (var session = _sessionProvider.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var deliveries = session.Query<Delivery>().Where(delivery => eventIds.Contains(delivery.EventId)).ToArray();
                foreach (var delivery in deliveries)
                {
                    delivery.WasRead = true;
                    session.Update(delivery);
                }

                transaction.Commit();
            }
        }

        private readonly DatabaseSessionProvider _sessionProvider;
    }
}