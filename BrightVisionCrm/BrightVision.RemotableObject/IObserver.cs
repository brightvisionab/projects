using System;

namespace BrightVision.RemotableObject {

    public interface IObserver {
        void Notify(int _import_file_id, int _confidence, string _confidence_operator, int _similarity, string _similarity_operator, short _execution_type);
    }
}
