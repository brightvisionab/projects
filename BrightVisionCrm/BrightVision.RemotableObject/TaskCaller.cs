using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

namespace BrightVision.RemotableObject {
    public class TaskCaller : MarshalByRefObject {

        public void MakeCall(int _import_file_id, int _confidence, string _confidence_operator, int _similarity, string _similarity_operator, short _execution_type) {
            Cache.GetInstance().SendMessage(_import_file_id, _confidence, _confidence_operator,_similarity, _similarity_operator, _execution_type);
        }
        public override object InitializeLifetimeService() {
            return null;
        }
    }
}
