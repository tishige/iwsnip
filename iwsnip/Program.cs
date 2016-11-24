using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using log4net;
using CommandLine;
using CommandLine.Text;
using CommandLine.Parsing;


namespace iwsnip
{

    class Options
    {
        //[Option('r', "read", Required = true,
        //  HelpText = "Input file to be processed.")]
        //public string InputFile { get; set; }

        [Option('l', "logfilename", HelpText = "ログファイル名")]
        public string logfilename { get; set; }

        [Option('s', "startdatetime", Required = true, HelpText = "開始日:20161010225859")]
        public string startdate { get; set; }

        [Option('e', "enddatetime", Required = true, HelpText = "終了日:20161010235859")]
        public string enddate { get; set; }

        [Option('i', "indir",  HelpText = "ininlogファイルディレクトリ")]
        public string indir { get; set; }

        [Option('o', "outdir",  HelpText = "SNIPファイル出力先ディレクトリ")]
        public string outdir { get; set; }

        [Option('p', "purge", DefaultValue = false, HelpText = "解凍ファイル削除")]
        public bool bpurge { get; set; }

        [Option('z', "zip", DefaultValue = false, HelpText = "SNIPファイルをZIP圧縮")]
        public bool bzip { get; set; }

        [Option('d', "delete", DefaultValue = false, HelpText = "ZIP済みのファイルを削除")]
        public bool bdelzip { get; set; }


        [Option('v', "verbose", DefaultValue = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }


    /// <summary>
    /// Logファイルを解凍してSNIP
    /// </summary>
    class Program
    {

        // Logger
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string DATE_FORMAT_ARG = "yyyyMMddHHmmss";

        /// <summary>
        /// メイン処理
        /// </summary>
        /// <param name="args"></param>

        static void Main(string[] args)
        {
            try
            {
                //引数チェック

                //CommandLine引数取得
                var options = new Options();
                if (CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    // Values are available here
                    //if (options.Verbose) Console.WriteLine("REP TEST");
                }

                //ログファイル名取得
                String logfilename = options.logfilename;
                if(options.logfilename==null)
                {
                    //logger.Error(Properties.Resources.LMSG_E_001_INV_ARG);
                    logfilename = "all";
                }
                //開始日取得
                DateTime startdatetime;
                if (!DateTime.TryParseExact(options.startdate, DATE_FORMAT_ARG, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out startdatetime))
                {
                    logger.Error(Properties.Resources.LMSG_E_002_INV_DATE);
                    return;
                }
                //終了日取得
                DateTime enddatetime;
                if (!DateTime.TryParseExact(options.enddate, DATE_FORMAT_ARG, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out enddatetime))
                {
                    logger.Error(Properties.Resources.LMSG_E_002_INV_DATE);
                    return;
                }
                //終了日の方が後か
                if (startdatetime >= enddatetime)
                {
                    logger.Error(Properties.Resources.LMSG_E_002_INV_DATE);
                    return;
                }
                //ininlogのパス取得
                String logdirectry = options.indir;
                if (logdirectry==null)
                {
                    logdirectry=Properties.Settings.Default.inlogdir;

                }
                else
                {
                    if (logdirectry.EndsWith("\\")==false)
                    {
                        logdirectry = logdirectry + "\\";
                    }

                }
                //ininlogのパスを日付から生成
                String logdate = startdatetime.ToString("yyyy-MM-dd");
                logdirectry = logdirectry + logdate+"\\";

                //ログディレクトリの存在確認
                if (Directory.Exists(logdirectry)==false)
                {
                    logger.Error("ログディレクトリが存在しません");
                    Environment.Exit(0);
                    return;
                }

                //出力先ディレクトリの生成
                String outdirectry = options.outdir;
                if(outdirectry==null)
                {
                    outdirectry = System.Environment.CurrentDirectory+"\\";
                }
                else
                {
                    if (outdirectry.EndsWith("\\") == false)
                    {
                        outdirectry = outdirectry + "\\";
                    }
                    if(Directory.Exists(outdirectry) ==false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(outdirectry);
                        }
                        catch (Exception e)
                        {
                            logger.Error("出力先ディレクトリの作成に失敗しました");
                            Environment.Exit(0);
                        }
                    }

                }

                //ZIP解凍後にininlogファイルを削除するか True=削除　False=削除せず(Default)
                Boolean purgelog = options.bpurge;

                //SNIP後にininlogファイルを圧縮するか True=圧縮　False=圧縮せず(Default)
                Boolean ziplog = options.bzip;

                //ZIP後にSNIPしたininlogファイルを削除するか True=削除　False=削除せず(Default)
                Boolean deletesnippedlog = options.bdelzip;


                //開始
                logger.Info(Properties.Resources.LMSG_I_001_START);
                logger.Info("対象ログファイル " + logfilename);
                logger.Info("開始日時 " + startdatetime.ToString());
                logger.Info("終了日時 " + enddatetime.ToString());
                logger.Info("ログディレクトリ " + logdirectry);
                logger.Info("出力先ログディレクトリ " + outdirectry);
                logger.Info("ininlog削除 " + purgelog);
                logger.Info("SNIPファイルを圧縮 " + ziplog);

                AnalyzeJournal AnalyzeJournal = new AnalyzeJournal();
                ExtractZIP extractzip = new ExtractZIP();
                logsnipper logsnipper = new logsnipper();

                //ininjournalファイルから対象ファイル名を取得
                string big4value = Properties.Settings.Default.big4;
                string[] big4array = null;
                string ininlogfullpath = null;
                List<string> ininloglist = new List<string>();
                List<string> snippedfilelist = new List<string>();
                List<string> snippedfilelistjournalidx = new List<string>();


                //SNIP対象のログファイル名設定
                if (logfilename == "all")
                {
                    big4array = big4value.Split(',');
                }
                else
                {
                    big4array = new string[1];
                    big4array[0] = logfilename.ToString();
                }

                //SNIPするininlogファイル名をjournalファイルから取得しininloglist配列にセット
                foreach (string logpath in big4array)
                {
                    ininlogfullpath = logdirectry + logpath;
                    ininloglist.AddRange(AnalyzeJournal.getininlogfilename(ininlogfullpath, startdatetime, enddatetime));
                }

                if (ininloglist==null)
                {
                    logger.Info("指定された日時のininlogファイルが存在しないため終了します");
                    Environment.Exit(0);
                }

                foreach (string ininlogfile in ininloglist)
                {
                    //存在するか
                    if (File.Exists(ininlogfile))
                    {
                        //SNIP実行
                        snippedfilelist.Add(logsnipper.snip(ininlogfile, startdatetime, enddatetime, outdirectry));
                    }
                    else
                    {
                        //zipになっているか
                        string zipfilename = Path.GetDirectoryName(ininlogfile) + "\\" + Path.GetFileNameWithoutExtension(ininlogfile) + ".zip";
                        if (File.Exists(zipfilename))
                        {
                            //ininlogと同じ場所にzipを解凍
                            extractzip.Unzip(zipfilename, logdirectry);
                            //SNIP実行
                            snippedfilelist.Add(logsnipper.snip(ininlogfile, startdatetime, enddatetime, outdirectry));
                        }
                        else
                        {
                            //zipにもなっていないのでスキップ
                            logger.Info("指定された日時のininlogファイルが存在しないためスキップします");
                        }
                    }

                    if(purgelog)
                    {
                        if (logdate != System.DateTime.Now.ToString("yyyy-MM-dd"))
                        {
                            File.Delete(ininlogfile);
                            logger.Info(ininlogfile + " を削除しました");
                        }
                        else
                        {
                            logger.Info(ininlogfile + " は当日のため削除できません");
                        }
                    }
                }

                logger.Info("SNIP済みファイル一覧");
                foreach(string sniplist in snippedfilelist)
                {
                    logger.Info(sniplist);
                }
                //ZIP化するか
                if(ziplog)
                {
                    logger.Info("SNIP済みファイル圧縮開始");
                    //SNIPされたファイル名にindexファイルとjournalファイル拡張子をつけたファイル名をリストに追加
                    snippedfilelistjournalidx.AddRange(snippedfilelist);
                    foreach(string snippedlist in snippedfilelist)
                    {
                        string indexfile = Path.GetDirectoryName(snippedlist) + "\\" + Path.GetFileNameWithoutExtension(snippedlist) + ".ininlog.ininlog_idx";
                        string journalfile = Path.GetDirectoryName(snippedlist) + "\\" + Path.GetFileNameWithoutExtension(snippedlist) + ".ininlog_journal";
                        snippedfilelistjournalidx.Add(indexfile);
                        snippedfilelistjournalidx.Add(journalfile);
                    }

                    //ZIPファイル名生成
                    string zipfilename = outdirectry + "\\" + logdate + "_" + logfilename + ".zip";
                    //ininlog+idx+journalファイルをすべて圧縮
                    Boolean createzipresult= extractzip.createzip(snippedfilelistjournalidx, zipfilename);
                    logger.Info("SNIP済みファイル圧縮終了");
                    //SNIPされたファイルを削除がTRUE　かつ　ZIPファイル存在　かつ　ZIPが成功した場合　ZIPファイルを削除
                    //すでにZIPされた同名ファイルが存在して処理を中断した場合にSNIPされたファイルが削除されることを回避
                    if(deletesnippedlog && File.Exists(zipfilename) && createzipresult)
                    {
                        foreach (string sniplist in snippedfilelistjournalidx)
                        {
                            //SNIPされたファイルを削除
                            File.Delete(sniplist);
                            logger.Info(sniplist + " を削除しました");
                        }
                    }
                }
            }
            catch (Exception e)
            {

                logger.Error(String.Format(Properties.Resources.LMSG_E_999_ACCIDENTAL, e.Message), e);

            }
            finally
            {
                logger.Info(Properties.Resources.LMSG_I_002_END);

            }

        }

        /// <summary>
        /// Usageを出力します。
        /// </summary>
        private static void printUsage()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Usage:");
            sb.AppendLine("iwsnip.exe -l ログファイル名　-s 開始日時　-e 終了日時");
            sb.AppendLine("ログファイル名(省略時)：ts/IP/sip/notifier/cs/sm を対象");
            sb.AppendLine("ログファイル名(指定時)：tsserver");
            sb.AppendLine("ログファイル名(指定時)：スペースを含む場合は\"\"で囲みます　例　\"recorder server\" ");
            sb.AppendLine("開始日 終了日：yyyyMMddHHmmss形式で指定");
            sb.AppendLine("ログディレクトリ(省略可)：-i d:\\i3\\ic\\data\\");
            sb.AppendLine("出力先ディレクトリ(省略可)：-o d:\\data\\");
            sb.AppendLine("解凍したininlogファイルを削除(省略すると削除せず)：-p");
            sb.AppendLine("SNIPしたininlogファイルを圧縮(省略すると圧縮せず)：-z");
            sb.AppendLine("圧縮した後SNIPしたファイルを削除(省略すると削除せず)：-d");
            sb.AppendLine("");
            sb.AppendLine("Example:");
            sb.AppendLine("TS を圧縮");
            sb.AppendLine("iwsnip -l tsserver -s 20161201100115 -e 20161201100215");
            sb.AppendLine("BIG4などALLに設定したプロセスを圧縮");
            sb.AppendLine("iwsnip -l all -s 20161201100115 -e 20161201100215");
            sb.AppendLine("BIG4などALLに設定したプロセスを圧縮しZIP化してSNIPファイルを削除");
            sb.AppendLine("iwsnip -l all -s 20161201100115 -e 20161201100215 -z -d");

            Console.Out.Write(sb.ToString());

        }

    }
}
