using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CommandLine;
using FileWatcher.Models;

namespace FileWatcher
{
    public class Program
    {
        #region Public Methods

        public static int Main(string[] args)
        {
            using (var parser = new Parser(config => config.HelpWriter = Console.Out))
            {
                parser.ParseArguments<Options>(args)
                      .WithParsed(options =>
                                  {
                                      var tokenSource = new CancellationTokenSource();
                                      var token = tokenSource.Token;
                                      options = AdjustDirectory(options);
                                      if (OptionsValidator.Instance.Validate(options))
                                      {
                                          new TimerMonitor(options, Console.WriteLine, token).BeginMonitor();

                                          while (true)
                                          {
                                              var c = Console.ReadKey(true);
                                              if (char.ToLower(c.KeyChar).Equals('q'))
                                              {
                                                  tokenSource.Cancel(true);
                                                  break;
                                              }
                                          }
                                      }
                                      else
                                      {
                                          ShowErrors(null);
                                      }
                                  })
                      .WithNotParsed(ShowErrors);
            }

            return 0;
        }

        #endregion

        #region Private Methods

        private static Options AdjustDirectory(Options options)
        {
            // if not rooted, then assume relative
            return !Path.IsPathRooted(options.FilePath) ? new Options {FilePath = Path.Combine(Assembly.GetExecutingAssembly().Location, options.FilePath).ToLower(), FilePattern = options.FilePattern.ToLower()} : options;
        }

        private static void ShowErrors(IEnumerable<Error> errors)
        {
            if (OptionsValidator.Instance.Errors.Count > 0)
            {
                foreach (var error in OptionsValidator.Instance.Errors)
                {
                    Console.WriteLine(error);
                }
            }

            var errs = errors.ToList();
            if (errs.Any())
            {
                foreach (var error in errs)
                {
                    Console.WriteLine(error);
                }
            }
        }

        #endregion
    }
}