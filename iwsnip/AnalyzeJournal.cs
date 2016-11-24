using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace iwsnip
{
    class AnalyzeJournal
    {
        // Logger
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

//        public String[] getininlogfilename(string journalfilename, DateTime starttime, DateTime endtime)
        public List<string> getininlogfilename(string journalfilename, DateTime starttime, DateTime endtime)

        {

            logger.Info("Jornalファイル" + journalfilename + " 解析開始");

            //ininjournalファイル存在チェック
            if (!File.Exists(journalfilename + ".ininlog_journal"))
            {
                logger.Error(journalfilename + " は指定された日時のファイルがありません");
                return null;
            }
            else
            { 
                string[] lines = File.ReadAllLines(journalfilename + ".ininlog_journal");
                var elems = lines.Select(x => x.Split('\t'));

                // GroupByでキーと返してほしい要素を指定する。返してほしい要素は無名クラスを使用（string[]でも可）。
                // Whereで絞っても複数行ある可能性があるのでFirstOrDefaultでstringにしている
                var newgroups = elems.GroupBy(x => x[4],
                    (key, group) => new {
                        start = group.Where(row => row[3] == "Start").Select(row => row[0]).FirstOrDefault(),
                        end = group.Where(row => row[3] == "End").Select(row => row[0]).FirstOrDefault()
                                ?? group.Where(row => row[3] == "Start").Select(row => row[0]).FirstOrDefault().Substring(0, 11) + "23:59:59",
                        // ↑EndがNullだったらStartの日付に時刻付加
                        file = key
                    });

                var filterednewgroups = newgroups
                    .Where(x =>
                    (DateTime.ParseExact(x.start, "yyyy/MM/dd HH:mm:ss", null) <= endtime) &&
                    (DateTime.ParseExact(x.end, "yyyy/MM/dd HH:mm:ss", null) >= starttime)
                    );

                //string[] filteredresult = filterednewgroups.Select(x => x.file.ToString()).ToArray();
                List<string> filteredresult = filterednewgroups.Select(x => x.file.ToString()).ToList();

                logger.Info("Jornalファイル" + journalfilename + " 解析結果");
                foreach (string result in filteredresult)
                {
                    logger.Info(result);
                }
                logger.Info("Jornalファイル" + journalfilename + " 解析終了");
                return filteredresult;
            }
        }

    }
}
