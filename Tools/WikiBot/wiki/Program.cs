using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWikiBot;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Concurrent;

namespace wiki
{
    class Program : Bot
    {
        static ConcurrentStack<Page> stack;
        static int cnt;
        static readonly Regex pattern = new Regex(@"{{#invoke:Home\|infobox\|.*?}}\s*{{#invoke:Home\|des\|.*?}}\s*(.*?)\s*{{#invoke:Home\|level\|.*?}}", RegexOptions.Singleline);
        static void Main(string[] args)
        {
            var wiki = new Wiki();
            var pageList = wiki.GetCategoryMembers("功法");
            stack = new ConcurrentStack<Page>();
            foreach (Page page in pageList) stack.Push(page);
            cnt = 0;
            for (int i = 0; i < 8; i++)
            {
                Thread thread = new Thread(ThreadMethod)
                {
                    Name = $"线程{i + 1}"
                };
                thread.Start();
            }
            //ThreadMethod();
            Console.ReadKey();
        }
        static void ThreadMethod()
        {
            while (stack.Count > 0)
            {
                if(!stack.TryPop(out var page))
                {
                    Console.WriteLine($"{Thread.CurrentThread.Name}: TryPop failed");
                    return;
                }
                var thisCnt = ++cnt;
                redo:;
                try
                {
                    page.Load();
                    Console.WriteLine($"{Thread.CurrentThread.Name}: 执行第{thisCnt}个页面 -- {page.title}");
                    page.text = page.text.Replace("{{功法", "{{功法页面");
                    Console.WriteLine(page);
                    page.Save();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{Thread.CurrentThread.Name}: " + e.Message);
                    Wait(1);
                    goto redo;
                }
            }
            Console.WriteLine($"{Thread.CurrentThread.Name}: 执行结束");
        }
        public class Wiki
        {
            readonly Site site = new Site("https://taiwu.huijiwiki.com/", "とあるBOT", "toarubot");
            Page page;
            PageList pl;
            public void GotoPage(string pageName)
            {
                page = new Page(site, pageName);
                page.Load();
            }
            public Page Get(string pageName = null)
            {
                if (pageName != null) GotoPage(pageName);
                return page;
            }
            public PageList GetCategoryMembers(string category)
            {
                pl = new PageList(site);
                pl.FillFromCategoryTree(category);
                //pl.FilterNamespaces(new int[] { 0 });
                //pl.Load();
                return pl;
            }
            public void RefreshPage()
            {
                page.Save();
            }
            public void RefreshAll()
            {
                foreach (Page page in pl)
                {
                    page.Save();
                }
            }
        }
    }
}

