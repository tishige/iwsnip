using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace iwsnip
{
    class logsnipper
    {
        // Logger
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string snip(string ininlogfilename,DateTime startdatetime, DateTime enddatetime,string outdir)
        {
            //logsnipexeパス取得
            string logsnippath;
            if (File.Exists(Properties.Settings.Default.logsnipver4path))
            {
                //Ver4.0/201xの場合
                logsnippath = Properties.Settings.Default.logsnipver4path;
            }
            else if (File.Exists(Properties.Settings.Default.logsnipver3path))
            {
                //CIC3.0の場合
                logsnippath = Properties.Settings.Default.logsnipver3path;
            }
            else
            {
                logger.Error("logsnip.exeが見つかりません");
                return null;
            }

            //開始終了日時生成
            String logdate = startdatetime.ToString("yyyy-MM-dd");
            String starttime = startdatetime.ToString("HH:mm:ss");
            String endtime = enddatetime.ToString("HH:mm:ss");


            //出力先ディレクトリ生成
            String outputstarttime = startdatetime.ToString("HHmmss");
            String outputendtime = enddatetime.ToString("HHmmss");
            String outputininfilename = Path.GetFileNameWithoutExtension(ininlogfilename);
 
            //引数生成
            starttime = logdate + "@"+starttime + ".000";
            endtime = logdate + "@" + endtime+".000";
            String outputdir = outdir + outputstarttime + "_" + outputendtime + "_" + outputininfilename+".ininlog";

            //logsnip実行
            logger.Info(outputininfilename　+　"　SNIP実行");
            string program = logsnippath;
            ProcessStartInfo psInfo = new ProcessStartInfo();
            psInfo.FileName = logsnippath;
            psInfo.CreateNoWindow = true;
            psInfo.UseShellExecute = false;
            psInfo.RedirectStandardOutput = true;
            psInfo.Arguments ="--log " + @""""+ ininlogfilename + @"""" + " --from " + starttime + " --to " + endtime + " --out " + @""""+ outputdir + @"""";

            Process extProcess = Process.Start(psInfo);
            string output = extProcess.StandardOutput.ReadToEnd();
            logger.Info("\r\n"+output);

            if (output.Contains("Writing")==false)
            {
                logger.Error("SNIP失敗");
            }

            logger.Info("SNIP処理終了");
            return outputdir;
        }

    }
}
