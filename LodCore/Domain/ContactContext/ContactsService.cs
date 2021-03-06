﻿using ContactContext.Events;
using Journalist;
using NotificationService;

namespace ContactContext
{
    public class ContactsService : IContactsService
    {
        public ContactsService(IEventSink contactsEventSink)
        {
            Require.NotNull(contactsEventSink, nameof(contactsEventSink));
            _contactsEventSink = contactsEventSink;
        }

        public void SendContactMessage(NewContactMessage contactMessage)
        {
            Require.NotNull(contactMessage, nameof(contactMessage));
            _contactsEventSink.ConsumeEvent(contactMessage);
        }

        private readonly IEventSink _contactsEventSink;
    }
}
