using System.Collections;
using System.Threading.Tasks;

namespace UnityEPL {

    public class VideoManager : EventMonoBehaviour {
        protected override void AwakeOverride() {
            throw new System.NotImplementedException();
        }

        public Task ShowVideo() {
            return DoWaitFor(ShowVideoHelper);
        }

        protected IEnumerator ShowVideoHelper() {
            yield break;
        }
    }

}