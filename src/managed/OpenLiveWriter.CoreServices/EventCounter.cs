// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OpenLiveWriter.CoreServices
{
    public class EventCounter
    {
        private Dictionary<string, List<StackTrace>> _eventsHooked = new Dictionary<string, List<StackTrace>>();
        private Dictionary<string, List<StackTrace>> _eventsUnhooked = new Dictionary<string, List<StackTrace>>();

        [Conditional("DEBUG")]
        public void EventHooked(MulticastDelegate eventHandler)
        {
            // The rest of the method must be inline or the stack trace won't be correct.
            StackTrace currentStackTrace = new StackTrace();

            // Make sure we were called from an "add" method of an event.
            string callingEventMethodName = currentStackTrace.GetFrame(1).GetMethod().Name;
            Debug.Assert(callingEventMethodName.StartsWith("add_", StringComparison.OrdinalIgnoreCase), "EventHooked called from an invalid method");
            string eventName = callingEventMethodName.Substring("add_".Length);

            // Save the current stack trace for debugging.
            if (!_eventsHooked.ContainsKey(eventName))
                _eventsHooked[eventName] = new List<StackTrace>();

            _eventsHooked[eventName].Add(currentStackTrace);
        }

        [Conditional("DEBUG")]
        public void EventUnhooked(MulticastDelegate eventHandler)
        {
            // The rest of the method must be inline or the stack trace won't be correct.
            StackTrace currentStackTrace = new StackTrace();

            // Make sure we were called from a "remove" method of an event.
            string callingEventMethodName = currentStackTrace.GetFrame(1).GetMethod().Name;
            Debug.Assert(callingEventMethodName.StartsWith("remove_", StringComparison.OrdinalIgnoreCase), "EventUnhooked called from an invalid method");
            string eventName = callingEventMethodName.Substring("remove_".Length);

            if (eventHandler == null)
            {
                // If the MulticastDelegate is null, then the event is completely unhooked.
                _eventsHooked.Remove(eventName);
                _eventsUnhooked.Remove(eventName);
            }
            else
            {
                // Save the current stack trace for debugging.
                if (!_eventsUnhooked.ContainsKey(eventName))
                    _eventsUnhooked[eventName] = new List<StackTrace>();

                _eventsUnhooked[eventName].Add(currentStackTrace);
            }
        }

        [Conditional("DEBUG")]
        public void AssertAllEventsAreUnhooked()
        {
            Debug.Assert(_eventsHooked.Keys.Count == 0, "Some events were not unhooked. See debug log for more details.");

            foreach (string eventName in _eventsHooked.Keys)
            {
                List<StackTrace> eventHookedStackTraces = _eventsHooked[eventName];

                List<StackTrace> eventUnhookedStackTraces = new List<StackTrace>();
                if (_eventsUnhooked.ContainsKey(eventName))
                    eventUnhookedStackTraces = _eventsUnhooked[eventName];

                Debug.WriteLine(eventName + " was left hooked after " + eventHookedStackTraces.Count + " hooks and " + eventUnhookedStackTraces.Count + " unhooks!");

                Debug.WriteLine(eventName + " hooks:");
                eventHookedStackTraces.ForEach(stackTrace => Debug.WriteLine(stackTrace));

                Debug.WriteLine(eventName + " unhooks:");
                eventUnhookedStackTraces.ForEach(stackTrace => Debug.WriteLine(stackTrace));
            }
        }

        [Conditional("DEBUG")]
        public void Reset()
        {
            _eventsHooked = new Dictionary<string, List<StackTrace>>();
            _eventsUnhooked = new Dictionary<string, List<StackTrace>>();
        }
    }
}
