﻿/*
      █▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀ ▀▀▀▀▀▀▀▀▀▀▀▀▀▀ ▀▀▀  ▀  ▀      ▀▀ 
      █   
      █      ▄███████▄                                                             
      █     ███    ███                                                             
      █     ███    ███    █████  ██████     ▄████▄     █████   ▄█████     ▄▄██▄▄▄  
      █     ███    ███   ██  ██ ██    ██   ██    ▀    ██  ██   ██   ██  ▄█▀▀██▀▀█▄ 
      █   ▀█████████▀   ▄██▄▄█▀ ██    ██  ▄██        ▄██▄▄█▀   ██   ██  ██  ██  ██ 
      █     ███        ▀███████ ██    ██ ▀▀██ ███▄  ▀███████ ▀████████  ██  ██  ██ 
      █     ███          ██  ██ ██    ██   ██    ██   ██  ██   ██   ██  ██  ██  ██ 
      █    ▄████▀        ██  ██  ██████    ██████▀    ██  ██   ██   █▀   █  ██  █  
      █
 ▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄ ▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄ ▄▄  ▄▄ ▄▄   ▄▄▄▄ ▄▄     ▄▄     ▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄ ▄ ▄ 
 █████████████████████████████████████████████████████████████ ███████████████ ██  ██ ██   ████ ██     ██     ████████████████ █ █ 
      ▄  
      █  The main application class.
      █  
      █▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀ ▀▀▀▀▀▀▀▀▀▀▀ ▀ ▀▀▀     ▀▀               ▀   
      █  The GNU Affero General Public License (GNU AGPL)
      █  
      █  Copyright (C) 2016 JP Dillingham (jp@dillingham.ws)
      █  
      █  This program is free software: you can redistribute it and/or modify
      █  it under the terms of the GNU Affero General Public License as published by
      █  the Free Software Foundation, either version 3 of the License, or
      █  (at your option) any later version.
      █  
      █  This program is distributed in the hope that it will be useful,
      █  but WITHOUT ANY WARRANTY; without even the implied warranty of
      █  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
      █  GNU Affero General Public License for more details.
      █  
      █  You should have received a copy of the GNU Affero General Public License
      █  along with this program.  If not, see <http://www.gnu.org/licenses/>.
      █  
      ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀  ▀▀ ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀██ 
                                                                                                   ██ 
                                                                                               ▀█▄ ██ ▄█▀ 
                                                                                                 ▀████▀   
                                                                                                   ▀▀                            */
using System;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using NLog;
using Symbiote.Core.Configuration;
using Symbiote.Core.Model;
using Symbiote.Core.Platform;
using Symbiote.Core.Plugin;
using Symbiote.Core.Service;

namespace Symbiote.Core
{
    /// <summary>
    /// The main application class.
    /// </summary>
    public class Program
    {
        #region Fields

        /// <summary>
        /// The main logger for the application.
        /// </summary>
        private static xLogger logger = (xLogger)LogManager.GetCurrentClassLogger(typeof(xLogger));

        /// <summary>
        /// The ProgramManager for the application.
        /// </summary>
        private static ProgramManager manager;

        /// <summary>
        /// The list of Managers for the application.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Each Manager must be listed in the order in which they are to be instantiated and started.  The order
        ///     will be reversed when the application stops.
        /// </para>
        /// <para>
        ///     Inter-Manager dependencies must be taken into consideration when determining the order.
        /// </para>
        /// </remarks>
        private static Type[] managers = new Type[]
        {
            typeof(PlatformManager),
            typeof(ConfigurationManager),
            typeof(PluginManager),
            typeof(ModelManager),
            typeof(ServiceManager)
        };

        #endregion

        #region Methods

        #region Internal Methods

        #region Internal Static Methods

        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Responsible for instantiating the platform and determining whether to start the application as a 
        ///     Windows service or console/interactive application.
        /// </para>
        /// <para>
        ///     The "-logLevel:*" argument is used to determine the logging level of the application.  Acceptable values are:
        ///     <list type="bullet">
        ///         <item>
        ///             <term>trace</term>
        ///             <description>The lowest logging level.  The output for this level is extremely verbose and only outputs to the log file.</description>
        ///         </item>
        ///         <item>
        ///             <term>debug</term>
        ///             <description>Basic debugging information.  These messages will appear in the console if this level is enabled.</description>
        ///         </item>
        ///         <item>
        ///             <term>info</term>    
        ///             <description>The default logging level; contains basic status information.</description>
        ///         </item>
        ///         <item>
        ///             <term>warn</term>
        ///             <description>Contains warning messages.</description>
        ///         </item>
        ///         <item>
        ///             <term>error</term>
        ///             <description>Contains error messages.  Typically errors produced on this level will not stop the application.</description>
        ///         </item>
        ///         <item>
        ///             <term>fatal</term>
        ///             <description>Fatal error messages; these errors stop the application.</description>
        ///         </item>
        ///     </list>
        ///     Note that the levels are additive; each level contains the messages associated with that level specifically as well as all "higher" (more severe) levels.
        /// </para>
        /// <para>
        ///     The "-(un)install-service" argument is used to install or uninstall the Windows service.  If either of these arguments is used, the application
        ///     performs the requested command and stops.  Re-run the application omitting the argument to start normally.
        /// </para>
        /// </remarks>
        /// <param name="args">Command line arguments.</param>
        internal static void Main(string[] args)
        {
            logger.Heading(LogLevel.Info, Assembly.GetExecutingAssembly().GetName().Name + " " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            logger.EnterMethod(xLogger.Params((object)args));
            logger.Info("Initializing application...");

            try
            {
                // process the command line arguments used to start the application
                logger.Debug("Program started with " + (args.Length > 0 ? "arguments: " + string.Join(", ", args) : "no arguments."));

                if (args.Length > 0)
                {
                    // check to see if logger arguments were supplied
                    string logarg = args.Where(a => Regex.IsMatch(a, "^((?i)-logLevel:)((?i)trace|debug|info|warn|error|fatal)$")).FirstOrDefault();
                    if (logarg != default(string))
                    {
                        // reconfigure the logger based on the command line arguments.
                        // valid values are "fatal" "error" "warn" "info" "debug" and "trace"
                        // supplying any value will disable logging for any level beneath that level, from left to right as positioned above
                        logger.Info("Reconfiguring logger to log level '" + logarg.Split(':')[1] + "'...");
                        Utility.SetLoggingLevel(logarg.Split(':')[1]);
                        logger.Info("Successfully reconfigured logger.");
                    }

                    // check to see if service install/uninstall arguments were supplied
                    string servicearg = args.Where(a => Regex.IsMatch(a, "^(?i)(-(un)?install-service)$")).FirstOrDefault();
                    if (servicearg != default(string))
                    {
                        string action = servicearg.Split('-')[1];
                        logger.Info("Attempting to " + action + " Windows Service...");

                        if (Utility.ModifyService(action))
                        {
                            logger.Info("Successfully " + action + "ed Windows Service.");
                        }
                        else
                        {
                            logger.Error("Failed to " + action + " Windows Service.");
                        }

                        // if we do anything with the service, do it then quit.  don't start the application if either argument was used.
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadLine();
                        return;
                    }
                }

                // instantiate the Program Manager.
                // the Program Manager acts as a Service Locator for the application; all of the various IManager instances specified in
                // the list of managers are instantiated (but not started) within the constructor of ProgramManager.
                logger.Heading(LogLevel.Debug, "Initialization");
                logger.Debug("Instantiating the Program Manager...");

                manager = ProgramManager.Instantiate(managers);

                logger.Debug("The Program Manager was instantiated successfully.");

                // determine whether the application is being run as a Windows service or as a console application and start accordingly.
                // it is possible to run Windows services on unix using mono-service, however this functionality is currently TBD.
                if ((PlatformManager.GetPlatformType() == PlatformType.Windows) && (!Environment.UserInteractive))
                {
                    logger.Info("Starting the application in service mode...");
                    ServiceBase.Run(new WindowsService());
                }
                else
                {
                    logger.Info("Starting the application in interactive mode...");
                    Start(args);
                    Stop();
                }
            }
            catch (Exception ex)
            {
                logger.Fatal("The application failed to initialize.");
                logger.Exception(LogLevel.Fatal, ex);
            }
            finally
            {
                logger.ExitMethod();
            }
        }

        /// <summary>
        /// Entry point for the application logic.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        internal static void Start(string[] args)
        {
            logger.EnterMethod(xLogger.Params((object)args));
            logger.Heading(LogLevel.Debug, "Startup");

            // this is the main try/catch for the application logic.  If an unhandled exception is thrown
            // anywhere in the application it will be caught here and treated as a fatal error, stopping the application.
            try
            {
                // start the program manager.
                logger.SubHeading(LogLevel.Debug, "Program Manager");
                logger.Info("Invoking the Program Manager Start routine...");
                Result managerStartResult = manager.StartManager(manager);

                if (managerStartResult.ResultCode == ResultCode.Failure)
                {
                    throw new Exception("The Program Manager failed to start: " + managerStartResult.LastErrorMessage());
                }

                logger.Info("Program Manager started.");
                logger.Info("Performing startup tasks...");

                Startup();
                
                logger.Info(manager.ProductName + " is running.");

                Console.ReadLine();
            }
            catch (TargetInvocationException ex)
            {
                logger.Fatal(ex, "Unable to start the web server.  Is " + manager.ProductName + " running under an account with administrative privilege?");
            }
            catch (Exception ex)
            {
                logger.Exception(LogLevel.Fatal, ex);
                if (!Environment.UserInteractive)
                {
                    throw;
                }
            }
            finally
            {
                logger.ExitMethod();
            }
        }

        /// <summary>
        /// Exit point for the application logic.
        /// </summary>
        internal static void Stop()
        {
            logger.EnterMethod();
            logger.Heading(LogLevel.Debug, "Shutdown");
            logger.Info(manager.ProductName + " is stopping...");

            try
            {
                manager.Stop(StopType.Shutdown);
                logger.Info("Program Manager stopped.");

                logger.Info("Performing shutdown tasks...");
                Shutdown();

                logger.Info(manager.ProductName + " stopped.");
            }
            catch (Exception ex)
            {
                logger.Exception(LogLevel.Error, ex);
            }
            finally
            {
                logger.ExitMethod();
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region Private Static Methods

        /// <summary>
        /// Miscellaneous startup tasks.
        /// </summary>
        private static void Startup()
        {
            Guid guid = logger.EnterMethod(true);

            // attach the Platform connector items to the model
            // detatch anything in "Symbiote.System.Platform" that was loaded from the config file
            logger.Info("Detatching potentially stale Platform items...");
            manager.GetManager<ModelManager>().RemoveItem(manager.GetManager<ModelManager>().FindItem(manager.InstanceName + ".System.Platform"));

            logger.Info("Attaching new Platform items...");

            // find or create the parent for the Platform items
            Item systemItem = manager.GetManager<ModelManager>().FindItem(manager.InstanceName + ".System");
            if (systemItem == default(Item))
            {
                systemItem = manager.GetManager<ModelManager>().AddItem(new Item(manager.InstanceName + ".System")).ReturnValue;
            }

            // attach the Platform items to Symbiote.System
            manager.GetManager<ModelManager>().AttachItem(manager.GetManager<PlatformManager>().Platform.Connector.Browse(), systemItem);
            logger.Info("Attached Platform items to '" + systemItem.FQN + "'.");

            Item symItem = manager.GetManager<ModelManager>().FindItem(manager.InstanceName);
            if (symItem == default(Item))
            {
                symItem = manager.GetManager<ModelManager>().AddItem(new Item(manager.InstanceName)).ReturnValue;
            }

            // show 'em what they've won!
            Utility.PrintLogo(logger);
            Utility.PrintModel(logger, manager.GetManager<ModelManager>().Model, 0, null, true);
            Utility.PrintLogoFooter(logger);

            logger.ExitMethod(guid);
        }

        /// <summary>
        /// Miscellaneous shutdown tasks.
        /// </summary>
        private static void Shutdown()
        {
            Guid guid = logger.EnterMethod(true);
            logger.ExitMethod(guid);
        }

        #endregion

        #endregion

        #endregion
    }
}