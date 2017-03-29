using System.Text;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC.Misc
{
    /// <summary>
    /// Contains extension methods for System.Web.Mvc.HtmlHelper.
    /// </summary>
    public static class BlueChilliMotifHtmlHelper
    {
        /// <summary>
        /// Returns HTML string for BlueChilli ASCII code.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <returns>An HTML-encoded string.</returns>
        public static MvcHtmlString BlueChilliAsciiMotif(this HtmlHelper html)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!--");
            sb.AppendLine("                                                                                 +");
            sb.AppendLine("                                                                                 +");
            sb.AppendLine("                                                                                 @");
            sb.AppendLine("                                                                                 @");
            sb.AppendLine("                                                                                 @");
            sb.AppendLine("    BBBBBBb    LLl                      cCCCc   hHH       II  LLl lLL  II        @");
            sb.AppendLine("    BBBBBBBBb  LLl                    cCCCCCCC  hHH       II  LLl lLL  II       #@");
            sb.AppendLine("    BBB   bBB  LLl                    CCC  cCCc hHH           LLl lLL          @@@");
            sb.AppendLine("    BBB    BB  LLl uUU   UU   eEEEe  cCC    cCC hHHhHHH   II  LLl lLL  II     ::::.");
            sb.AppendLine("    BBB   bBB  LLl uUU   UU  EEEEEEe CCC        hHHHHHHH  II  LLl lLL  II     ::::");
            sb.AppendLine("    BBBBBBBB   LLl uUU   UU eEE   EE CCc        hHH  hHH  II  LLl lLL  II     ::::");
            sb.AppendLine("    BBBbbbBBB  LLl uUU   UU EEeeeeEE CCc        hHH   HH  II  LLl lLL  II     :::,");
            sb.AppendLine("    BBB    BBb LLl uUU   UU EEEEEEEE CCc    ccc hHH   HH  II  LLl lLL  II     :::");
            sb.AppendLine("    BBB    BBb LLl uUU  uUU EEe      cCC    CCC hHH   HH  II  LLl lLL  II     ::`");
            sb.AppendLine("    BBBbbbBBB  LLl uUU  UUU eEE  eEE  CCCc cCCc hHH   HH  II  LLl lLL  II     ::,");
            sb.AppendLine("    BBBBBBBBb  LLl  UUUUUUU  eEEEEEe  cCCCCCCc  hHH   HH  II  LLl lLL  II     ::");
            sb.AppendLine("    bbbbbbbb   lll   uuuuuu   eEEee     cCCC    hhh   hh  ii  lll lLL  ii     ::");
            sb.AppendLine("                                                                              ::");
            sb.AppendLine("                                                                              :");
            sb.AppendLine("    -->");
            return MvcHtmlString.Create(sb.ToString());
        }
    }                                                                                    
}