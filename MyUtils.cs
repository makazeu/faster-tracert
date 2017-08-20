using System.Text;

namespace faster_tracert {
    public class MyUtils {
        public static string ConvertGb2312ToUtf8(byte[] gBytes) {
            var gb2312 = Encoding.GetEncoding("GB2312");
            var utf8 = Encoding.GetEncoding("UTF-8");
            var uBytes = Encoding.Convert(gb2312, utf8, gBytes);
            return utf8.GetString(uBytes);
        } 
    }
}