using System;

namespace BrightVision.RemotableObject {
    public class Cache {
        private static Cache m_oInstance;
        public static IObserver Observer;

        private Cache() { }

        public static void Attach(IObserver observer) {
            Observer = observer;
        }
        public static Cache GetInstance() {
            if (m_oInstance == null) {
                m_oInstance = new Cache();
            }
            return m_oInstance;
        }
        public void SendMessage(int _import_file_id, int _confidence, string _confidence_operator, int _similarity, string _similarity_operator, short _execution_type) {
            Observer.Notify(_import_file_id, _confidence, _confidence_operator,_similarity, _similarity_operator, _execution_type);
        }
    }    
}
