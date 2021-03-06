﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Immanuel.Ffmpeg.Controllers
{
    public class FileController : ApiController
    {
        async void CountIncrement()
        {
            //actOnValue('piiofpr9', 'VisitCnt', 'increment');
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    "https://keyvalue.immanuel.co/api/KeyVal/ActOnValue/piiofpr9/VisitCnt/increment",
                     new StringContent(""));
            }
        }

        static int GlobalCnt = 0;
        [Route("api/File/DownloadFile/{lng}")]
        [HttpGet()]
        public HttpResponseMessage DownloadFile(string lng)
        {
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/dwld");
            if (lng == "curl")
                path = Path.Combine(path, "a7_curl.txt");
            else if (lng == "cs")
                path = Path.Combine(path, "a7_cs.txt");
            else if (lng == "js")
            {
                path = Path.Combine(path, "a7_js.txt");
            }
            else if (lng == "java")
            {
                path = Path.Combine(path, "a7_java.txt");
            }
            else if (lng == "py")
            {
                path = Path.Combine(path, "a7_py.txt");
            }
            else if (lng == "android")
            {
                path = Path.Combine(path, "a7_android.txt");
            }
            else if (lng == "swift")
            {
                path = Path.Combine(path, "a7_swift.txt");
            }
            else if (lng == "objectivec")
            {
                path = Path.Combine(path, "a7_objc.txt");
            }
            else if (lng == "perl")
            {
                path = Path.Combine(path, "a7_perl.txt");
            }
            else
            {
                path = Path.Combine(path, "a7_curl.txt");
            }
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            byte[] arr = File.ReadAllBytes(path);
            response.Content = new StreamContent(new MemoryStream(arr));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            response.Content.Headers.ContentLength = arr.Length;
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.GetFileName(path)
            };

            return response;
        }

        string GetMimeTypes(string ext)
        {
            string mime = "";
            switch (ext)
            {
                case "flv":
                    mime = "video/x-flv";
                    break;
                case "mp4":
                    mime = "video/mp4";
                    break;
                case "avi":
                    mime = "video/avi";
                    break;
                case "wmv":
                    mime = "video/x-ms-wmv";
                    break;
                case "mpg":
                    mime = "video/mpeg";
                    break;
                case "mov":
                    mime = "video/quicktime";
                    break;
                case "m4v":
                    mime = "video/x-m4v";
                    break;
                case "mkv":
                    mime = "video/x-matroska";
                    break;
                case "webm":
                    mime = "video/webm";
                    break;
                case "3gp":
                    mime = "video/3gpp";
                    break;
            }
            return mime;
        }

        [HttpPost]
        public HttpResponseMessage Compress()
        {
            CountIncrement();
            ++GlobalCnt;
            string pPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Temp");
            System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;
            if (hfc.Count < 1)
            {
                throw new ApplicationException("Empty File Bad Request");
            }
            string totype = System.Web.HttpContext.Current.Request.Form["tofmt"] ?? "compress";
            string srctype = (Path.GetExtension(hfc[0].FileName) ?? "").Replace(".", "");
            var srcfile = GetFile(Path.Combine(GetDirectory(srctype, totype), hfc[0].FileName), 0);
            //string tofile = Path.ChangeExtension(srcfile, totype);
            hfc[0].SaveAs(srcfile);
            string convertedfile = srcfile;
            string convertedcompressdfile = Path.Combine(Path.GetDirectoryName(convertedfile), Path.GetFileNameWithoutExtension(convertedfile) + "_comp" + Path.GetExtension(convertedfile));
            //ffmpeg -i vid_big.mp4 -vcodec h264 -acodec aac vb_output.mp4 // Works great
            //ProcessExecute("-i \"" + convertedfile + "\" -vcodec h264 -acodec aac \"" + convertedcompressdfile + "\"");
            //ffmpeg -i vid_big.mp4 -vcodec h264 -b:v 1000k -acodec mp2 vid_big_t1.mp4 // Works Great 30mb to 3mb (Quality is good) - reducing 100k to 400k reduces to 30mb to 1.7mb (Qulaity is compromised)
            ProcessExecute("-i \"" + convertedfile + "\" -vcodec h264 -b:v 1000k -acodec mp2 \"" + convertedcompressdfile + "\"");
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            byte[] arr = File.ReadAllBytes(convertedcompressdfile);
            response.Content = new StreamContent(new MemoryStream(arr));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(GetMimeTypes(totype));
            response.Content.Headers.ContentLength = arr.Length;
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.GetFileNameWithoutExtension(hfc[0].FileName)
            };
            response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
            return response;
        }

        [HttpPost]
        public HttpResponseMessage ConverterAndCompress()
        {
            CountIncrement();
            ++GlobalCnt;
            string pPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Temp");
            System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;
            if (hfc.Count < 1)
            {
                throw new ApplicationException("Empty File Bad Request");
            }
            string totype = System.Web.HttpContext.Current.Request.Form["tofmt"] ?? "webm"; //FIX this post form
            string srctype = (Path.GetExtension(hfc[0].FileName) ?? "").Replace(".", "");
            var srcfile = GetFile(Path.Combine(GetDirectory(srctype, totype), hfc[0].FileName), 0);
            string tofile = Path.ChangeExtension(srcfile, totype);
            hfc[0].SaveAs(srcfile);
            string args = GetArgs(srcfile, tofile, srctype, totype);
            ProcessExecute(args);
            string convertedfile = tofile;
            string convertedcompressdfile = Path.Combine(Path.GetDirectoryName(convertedfile), Path.GetFileNameWithoutExtension(convertedfile) + "_comp" + Path.GetExtension(convertedfile));
            ProcessExecute("-i \"" + convertedfile + "\" -vcodec h264 -acodec aac \"" + convertedcompressdfile + "\"");
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            byte[] arr = File.ReadAllBytes(convertedcompressdfile);
            response.Content = new StreamContent(new MemoryStream(arr));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(GetMimeTypes(totype));
            response.Content.Headers.ContentLength = arr.Length;
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.GetFileNameWithoutExtension(hfc[0].FileName)
            };
            response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
            return response;
        }

        [HttpPost]
        public HttpResponseMessage Converter()
        {
            CountIncrement();
            ++GlobalCnt;
            string pPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Temp");
            System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;
            if (hfc.Count < 1)
            {
                throw new ApplicationException("Empty File Bad Request");
            }
            string totype = System.Web.HttpContext.Current.Request.Form["tofmt"] ?? "webm"; //FIX this post form
            string srctype = (Path.GetExtension(hfc[0].FileName) ?? "").Replace(".", "");
            var srcfile = GetFile(Path.Combine(GetDirectory(srctype, totype), hfc[0].FileName), 0);
            string tofile = Path.ChangeExtension(srcfile, totype);
            hfc[0].SaveAs(srcfile);
            string args = GetArgs(srcfile, tofile, srctype, totype);
            ProcessExecute(args);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            byte[] arr = File.ReadAllBytes(tofile);
            response.Content = new StreamContent(new MemoryStream(arr));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(GetMimeTypes(totype));
            response.Content.Headers.ContentLength = arr.Length;
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.GetFileNameWithoutExtension(hfc[0].FileName)
            };
            response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
            return response;
        }

        [HttpPost]
        public HttpResponseMessage ConvertToVids()
        {
            CountIncrement();
            ++GlobalCnt;
            string pPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Temp");
            System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;
            if (hfc.Count < 1)
            {
                throw new ApplicationException("Empty File Bad Request");
            }
            bool compress = System.Web.HttpContext.Current.Request.Form["compress"] == null ? false : Convert.ToBoolean(System.Web.HttpContext.Current.Request.Form["compress"]);
            bool onlycompress = System.Web.HttpContext.Current.Request.Form["onlycompress"] == null ? false : Convert.ToBoolean(System.Web.HttpContext.Current.Request.Form["onlycompress"]);
            string totype = System.Web.HttpContext.Current.Request.Form["tofmt"];
            string srctype = (Path.GetExtension(hfc[0].FileName) ?? "").Replace(".", "");
            var srcfile = GetFile(Path.Combine(GetDirectory(srctype, totype), hfc[0].FileName), 0);
            string tofile = Path.ChangeExtension(srcfile, totype);
            hfc[0].SaveAs(srcfile);
            string convertedfile = "";
            if (!onlycompress)
            {
                string args = GetArgs(srcfile, tofile, srctype, totype);
                ProcessExecute(args);
                convertedfile = tofile;
            } 
            else
            {
                convertedfile = srcfile;
            }
            if (compress)
            {
                string convertedcompressdfile = Path.Combine(Path.GetDirectoryName(convertedfile), Path.GetFileNameWithoutExtension(convertedfile) + "_comp" + Path.GetExtension(convertedfile));
                ProcessExecute("-i \"" + convertedfile + "\" -vcodec h264 -b:v 1000k -acodec mp2 \"" + convertedcompressdfile + "\"");
            }
            var datedir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(tofile)));
            var tt = System.Web.HttpUtility.UrlEncode(@"~/App_data/Temp/" + datedir + @"/" + srctype + "_" + totype + @"/" + Path.GetFileName(tofile));
            return new HttpResponseMessage()
            {
                Content = new JsonContent(new
                {
                    Path = tt,
                    FileName = Path.ChangeExtension(hfc[0].FileName, totype),
                    TotCnt = GlobalCnt
                })
            };
        }

        [HttpGet]
        public HttpResponseMessage GetCnt()
        {
            return new HttpResponseMessage()
            {
                Content = new JsonContent(new
                {
                    TotCnt = GlobalCnt
                })
            };
        }

        [HttpGet]
        public HttpResponseMessage GetFiles(string pth)
        {
            string pPath = System.Web.Hosting.HostingEnvironment.MapPath(pth);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            byte[] arr = File.ReadAllBytes(pPath);
            response.Content = new StreamContent(new MemoryStream(arr));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
            response.Content.Headers.ContentLength = arr.Length;
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.GetFileName(pth)
            };

            return response;
        }

        string GetDirectory(string srctype, string totype)
        {
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Temp");
            string tdir = (DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString());
            string fPath = Path.Combine(path, tdir, srctype + "_" + totype);
            if (!Directory.Exists(fPath))
            {
                Directory.CreateDirectory(fPath);
            }
            return fPath;
        }

        string GetFile(string fpath, int seq)
        {
            string i = "_" + seq.ToString();
            string fpath1 = Path.Combine(Path.GetDirectoryName(fpath), Path.GetFileNameWithoutExtension(fpath) + i.ToString() + Path.GetExtension(fpath));
            if (!File.Exists(fpath1))
            {
                return fpath1;
            }
            else
            {
                return GetFile(fpath, ++seq);
            }
        }

        static string ProcessExecute(string args)
        {
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data");
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = Path.Combine(path, "ffmpeg", "ffmpeg.exe");
                proc.StartInfo.Arguments = args;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                //Thread.Sleep(500);
                proc.Start();
                proc.WaitForExit();
                StringBuilder q = new StringBuilder();
                while (!proc.HasExited)
                {
                    q.Append(proc.StandardOutput.ReadToEnd());
                }
                string r = q.ToString();
                proc.Close();
                return r;
            }
        }

        static string GetArgs(string srcfile, string tofile, string stype, string ttype)
        {
            string args = "";
            //All mp4
            if (stype.ToLower() == "avi" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -c:v libx264 -crf 19 -preset slow -c:a aac -b:a 192k -ac 2 \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy  \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -vcodec libx264 -pix_fmt yuv420p -profile:v baseline -preset slow -crf 22 -movflags +faststart \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -b 5000k -acodec wmav2 -vcodec wmv2 -ar 44100 -ab 56000 -ac 2 -y \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -acodec copy -vcodec copy -f mov \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "mp4" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            // ALL AVI
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -vcodec libx264 -pix_fmt yuv420p -profile:v baseline -preset slow -crf 22 -movflags +faststart \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -b 5000k -acodec wmav2 -vcodec wmv2 -ar 44100 -ab 56000 -ac 2 -y \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -acodec copy -vcodec copy -f mov \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "avi")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "avi" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            //ALL WMV
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -acodec copy -vcodec copy -f mov \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "wmv")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "wmv" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            //ALL MPG
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -acodec copy -vcodec copy -f mov \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "mpg")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "mpg" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            //ALL MOV
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "mov")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "mov" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            //ALL M4V
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -vcodec copy -acodec copy \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "m4v")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "m4v" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            //ALL MkV
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -qscale 0 -ar 22050 -vcodec libx264 \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -c:v libx264 -ar 22050 -crf 28 \"" + tofile + "\"";
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "mkv")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "mkv" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            //ALL FLV
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "flv")
                args = "-i \"" + srcfile + "\" -qscale 0 \"" + tofile + "\"";
            else if (stype.ToLower() == "flv" && ttype.ToLower() == "webm")
                args = "-i \"" + srcfile + "\" -preset ultrafast \"" + tofile + "\"";
            //webm to WAV
            else if (stype.ToLower() == "webm" && ttype.ToLower() == "wav")
                args = "-i \"" + srcfile + "\" -acodec pcm_s16le -ac 2 \"" + tofile + "\"";
            //3gp to mp4
            else if (stype.ToLower() == "3gp" && ttype.ToLower() == "mp4")
                args = "-i \"" + srcfile + "\" -ab 64k -ar 44100 \"" + tofile + "\"";
            return args;
        }
    }

    public class JsonContent : HttpContent
    {

        private readonly MemoryStream _Stream = new MemoryStream();
        public JsonContent(object value)
        {

            Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var jw = new JsonTextWriter(new StreamWriter(_Stream));
            jw.Formatting = Formatting.Indented;
            var serializer = new JsonSerializer();
            serializer.Serialize(jw, value);
            jw.Flush();
            _Stream.Position = 0;

        }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return _Stream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _Stream.Length;
            return true;
        }
    }
}
