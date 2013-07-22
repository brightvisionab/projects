using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightVision.DQControl.Business {
    [Serializable]
    public class ContactAttendie {
        public bool Attending { get; set; }
        public int AccountID { get; set; }
        public int ContactID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public override bool Equals(object obj) {
            if (obj == null) return false;
            var att = obj as ContactAttendie;
            return (att.AccountID == AccountID &&
                att.ContactID == ContactID &&
                att.Name == Name);
        }
        public override int GetHashCode() {
            return AccountID.GetHashCode() ^ ContactID.GetHashCode() ^ Name.GetHashCode();
        }
    }
    public static class Extensions {
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action) {
            foreach (T item in enumeration) {
                action(item);
            }
        }
    }
}
