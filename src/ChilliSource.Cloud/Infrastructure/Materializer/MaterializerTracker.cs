using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Infrastructure.Materializer
{
    internal class MaterializerTracker : IDisposable
    {
        Dictionary<TrackerKey, object> _track = new Dictionary<TrackerKey, object>();

        private class TrackerKey
        {
            object _item;
            int _hashCode;
            public TrackerKey(object item)
            {
                if (item == null)
                    throw new ArgumentNullException("TrackerKey: item");

                _item = item;
                _hashCode = RuntimeHelpers.GetHashCode(_item);
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }

            public override bool Equals(object obj)
            {
                var cast = obj as TrackerKey;
                if (cast == null)
                    return false;

                return object.ReferenceEquals(this._item, cast._item);
            }
        }

        public bool BeginTrackObject(object item)
        {
            if (item == null)
                return false;

            var key = new TrackerKey(item);
            if (_track.ContainsKey(key))
                return false;

            //Value is irrelevant here
            _track[key] = null;
            return true;
        }

        public void Dispose()
        {
            if (_track != null)
            {
                _track.Clear();
                _track = null;
            }
        }
    }
}
