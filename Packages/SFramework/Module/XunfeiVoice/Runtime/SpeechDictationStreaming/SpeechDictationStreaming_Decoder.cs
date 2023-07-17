using System.Text;

namespace XunfeiVoice.Runtime
{
    public class SpeechDictationStreaming_Decoder
    {
        private SpeechDictationStreaming_ResponseResultDecoder[] texts;
        private int defc = 10;

        public SpeechDictationStreaming_Decoder()
        {
            this.texts = new SpeechDictationStreaming_ResponseResultDecoder[this.defc];
        }

        public void decode(SpeechDictationStreaming_ResponseResultDecoder speechDictationStreamingResponseResultDecoder)
        {
            if (speechDictationStreamingResponseResultDecoder.sn >= this.defc)
            {
                this.Resize();
            }

            if ("rpl".Equals(speechDictationStreamingResponseResultDecoder.pgs))
            {
                for (int i = speechDictationStreamingResponseResultDecoder.rg[0]; i <= speechDictationStreamingResponseResultDecoder.rg[1]; i++)
                {
                    this.texts[i].deleted = true;
                }
            }

            this.texts[speechDictationStreamingResponseResultDecoder.sn] = speechDictationStreamingResponseResultDecoder;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (SpeechDictationStreaming_ResponseResultDecoder t in this.texts)
            {
                if (t != null && !t.deleted)
                {
                    sb.Append(t.text);
                }
            }

            return sb.ToString();
        }

        public void Resize()
        {
            int oc = this.defc;
            this.defc <<= 1;
            SpeechDictationStreaming_ResponseResultDecoder[] old = this.texts;
            this.texts = new SpeechDictationStreaming_ResponseResultDecoder[this.defc];
            for (int i = 0; i < oc; i++)
            {
                this.texts[i] = old[i];
            }
        }

        public void Discard()
        {
            for (int i = 0; i < this.texts.Length; i++)
            {
                this.texts[i] = null;
            }
        }
    }


    public class SpeechDictationStreaming_ResponseResultDecoder
    {
        public int sn;
        public int bg;
        public int ed;
        public string text;
        public string pgs;
        public int[] rg;
        public bool deleted;
        public bool ls;
        public SpeechDictationStreaming_ResponseVAD vad;

        public override string ToString()
        {
            return "Text{" +
                   "bg=" + bg +
                   ", ed=" + ed +
                   ", ls=" + ls +
                   ", sn=" + sn +
                   ", text='" + text + '\'' +
                   ", pgs=" + pgs +
                   ", deleted=" + deleted +
                   '}';
        }
    }
}