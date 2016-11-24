using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using System.IO;

namespace iwsnip
{
    class ExtractZIP
    {
        // Logger
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public void Unzip(string logfilename,string extractpath)
        {
            try
            {
                var options = new ReadOptions { StatusMessageWriter = System.Console.Out };
                using (ZipFile zip = ZipFile.Read(logfilename, options))
                {
                    zip.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                    zip.ExtractAll(extractpath);
                }
            }
            catch (Exception ex1)
            {
                logger.Error(ex1);
                System.Console.Error.WriteLine("exception: " + ex1);
            }

        }

        public Boolean createzip(List<string> snippedfilename, string zipfilepath)
        {
            try
            {
                var options = new ReadOptions { StatusMessageWriter = System.Console.Out };
                using (ZipFile zip = new ZipFile())
                {
                    if (File.Exists(zipfilepath))
                    {
                        logger.Info(zipfilepath + " が存在しているため圧縮は行いません");
                        return false;
                    }

                    foreach(string checkexistingfile in snippedfilename)
                    {
                        string[] filelist = Directory.GetFiles(Path.GetDirectoryName(checkexistingfile), Path.GetFileNameWithoutExtension(checkexistingfile) + "_?.ininlog");
                        if (filelist.Length>0)
                        {
                            logger.Info(checkexistingfile + " に _ が付いた別ファイル名が存在しているため圧縮は行いません");
                            return false;
                        }
                    }
                    zip.AddFiles(snippedfilename);
                    zip.Save(zipfilepath);
                    logger.Info(zipfilepath + " 作成完了");
                }
            }
            catch (Exception ex1)
            {
                logger.Error(ex1);
                System.Console.Error.WriteLine("exception: " + ex1);
                return false;
            }
            //ZIP生成成功
            return true;

        }


    }
}
