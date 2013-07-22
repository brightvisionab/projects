using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BrightVision.Threading;

namespace BrightVision.EventLog {

    [Serializable]
    [XmlRoot("MessageQueue")]
    public class MessageQueue {
        private Queue<EventMessage> msgQueue = new Queue<EventMessage>();

        public MessageQueue() {
            
        }

        [XmlElement("Messages")]
        public EventMessage[] EventMessages {
            get {
                lock (msgQueue) {
                    EventMessage[] msQueues = new EventMessage[msgQueue.Count];
                    msgQueue.CopyTo(msQueues, 0);
                    return msQueues;
                }
            }
            set {
                if (value == null) return;

                lock (msgQueue) {                 
                    EventMessage[] msQueues = (EventMessage[])value;
                    msgQueue.Clear();
                    foreach (EventMessage mq in msQueues) {
                        msgQueue.Enqueue(mq);
                    }
                    Monitor.Pulse(msgQueue);
                }
            }
        }

        public void EnqueueMessage(EventMessage message) {
            lock (msgQueue) {
                msgQueue.Enqueue(message);
                Monitor.Pulse(msgQueue);
            }
        }

        public EventMessage DequeueMessage() {
            lock (msgQueue) {
                if (msgQueue.Count == 0)
                    Monitor.Wait(msgQueue);
                
                if (msgQueue.Count > 0)
                    return msgQueue.Dequeue();

                return null;
            }
        }

        
    }
}
