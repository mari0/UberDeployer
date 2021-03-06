﻿using System;
using System.Security.Principal;
using UberDeployer.CommonConfiguration;
using UberDeployer.ConsoleCommander;
using UberDeployer.Core.Deployment;
using UberDeployer.Core.Deployment.Pipeline;
using UberDeployer.Core.Domain;

namespace UberDeployer.ConsoleApp.Commands
{
  public class DeployCommand : ConsoleCommand
  {
    public DeployCommand(CommandDispatcher commandDispatcher)
      : base(commandDispatcher)
    {
    }

    public override int Run(string[] args)
    {
      if (args.Length != 4)
      {
        DisplayCommandUsage();

        return 1;
      }

      IProjectInfoRepository projectInfoRepository =
        ObjectFactory.Instance.CreateProjectInfoRepository();
      
      string projectName = args[0];
      string projectConfigurationName = args[1];
      string projectConfigurationBuildId = args[2];
      string targetEnvironmentName = args[3];      

      ProjectInfo projectInfo = projectInfoRepository.GetByName(projectName);

      if (projectInfo == null)
      {
        OutputWriter.WriteLine("Project named '{0}' doesn't exist.", projectName);
        return 1;
      }

      var deploymentInfo = new DeploymentInfo(
        projectName, 
        projectConfigurationName,
        projectConfigurationBuildId,
        targetEnvironmentName,
        projectInfo,
        null); //TODO MARIO create InputParams

      try
      {
        DeploymentTask deploymentTask =
          projectInfo.CreateDeploymentTask(ObjectFactory.Instance);

        deploymentTask.DiagnosticMessagePosted +=
          (eventSender, tmpArgs) => LogMessage(tmpArgs.Message);

        IDeploymentPipeline deploymentPipeline =
          ObjectFactory.Instance.CreateDeploymentPipeline();

        var deploymentContext = new DeploymentContext(RequesterIdentity);

        deploymentPipeline.StartDeployment(deploymentInfo, deploymentTask, deploymentContext);

        return 0;
      }
      catch (Exception exc)
      {
        LogMessage("Error: " + exc);

        return 1;
      }
    }

    public override void DisplayCommandUsage()
    {
      OutputWriter.WriteLine("Usage: {0} project projectConfiguration buildId targetEnvironment", CommandName);
    }

    private void LogMessage(string message)
    {
      OutputWriter.WriteLine(message);
    }

    public override string CommandName
    {
      get { return "deploy"; }
    }

    private static string RequesterIdentity
    {
      get
      {
        var windowsIdentity = WindowsIdentity.GetCurrent();

        if (windowsIdentity == null)
        {
          throw new InternalException("Couldn't get requester identity.");
        }

        return windowsIdentity.Name;
      }
    }
  }
}
