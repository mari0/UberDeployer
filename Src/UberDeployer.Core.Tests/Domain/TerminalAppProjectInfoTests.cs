﻿using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Management.NtServices;
using UberDeployer.Core.Management.ScheduledTasks;

namespace UberDeployer.Core.Tests.Domain
{
  // TODO IMM HI: formatting; code style
  [TestFixture]
  public class TerminalAppProjectInfoTests
  {
    private const string _Name = "name";
    private const string _ArtifactsRepositoryName = "artifRepoName";
    private const string _ArtifactsRepositoryDirName = "artifRepoDirName";
    private const bool _ArtifactsAreNotEnvirionmentSpecific = false;
    private const string _TerminalAppName = "terminalAppName";
    private const string _TerminalAppDirName = "terminalAppDirName";
    private const string _TerminalAppExeName = "terminalAppExeName";

    private static readonly List<EnvironmentUser> _EnvironmentUsers =
      new List<EnvironmentUser>
        {
          new EnvironmentUser("Sample.User", "some_user@centrala.kaczmarski.pl"),
        };

    private static readonly List<ProjectToFailoverClusterGroupMapping> _ProjectToFailoverClusterGroupMappings =
      new List<ProjectToFailoverClusterGroupMapping>
        {
          new ProjectToFailoverClusterGroupMapping("prj1", "cg1"),
        };

    [Test]
    public void Test_TerminalAppProjectInfo_Thows_When_Name_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
          {
            new TerminalAppProjectInfo(
            null,
            _ArtifactsRepositoryName,
            _ArtifactsRepositoryDirName,
            _ArtifactsAreNotEnvirionmentSpecific,
            _TerminalAppName,
            _TerminalAppDirName,
            _TerminalAppExeName);
          });
    }

    [Test]
    public void Test_TerminalAppProjectInfo_Thows_When_ArtifactsRepositoryName_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
        {
          new TerminalAppProjectInfo(
          _Name,
          null,
          _ArtifactsRepositoryDirName,
          _ArtifactsAreNotEnvirionmentSpecific,
          _TerminalAppName,
          _TerminalAppDirName,
          _TerminalAppExeName);
        });
    }

    [Test]
    public void Test_TerminalAppProjectInfo_Thows_When_TerminalAppName_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
        {
          new TerminalAppProjectInfo(
          _Name,
          _ArtifactsRepositoryName,
          _ArtifactsRepositoryDirName,
          _ArtifactsAreNotEnvirionmentSpecific,
          null,
          _TerminalAppDirName,
          _TerminalAppExeName);
        });
    }

    [Test]
    public void Test_TerminalAppProjectInfo_Thows_When_TerminalAppDirName_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
        {
          new TerminalAppProjectInfo(
          _Name,
          _ArtifactsRepositoryName,
          _ArtifactsRepositoryDirName,
          _ArtifactsAreNotEnvirionmentSpecific,
          _TerminalAppName,
          null,
          _TerminalAppExeName);
        });
    }

    [Test]
    public void Test_TerminalAppProjectInfo_Thows_When_TerminalAppExeName_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
        {
          new TerminalAppProjectInfo(
          _Name,
          _ArtifactsRepositoryName,
          _ArtifactsRepositoryDirName,
          _ArtifactsAreNotEnvirionmentSpecific,
          _TerminalAppName,
          _TerminalAppDirName,
          null);
        });
    }

    [Test]
    public void Test_CreateDeployemntTask_Throws_WhenObjectFactory_null()
    {
      var projectInfo = new TerminalAppProjectInfo(
            _Name,
            _ArtifactsRepositoryName,
            _ArtifactsRepositoryDirName,
            _ArtifactsAreNotEnvirionmentSpecific,
            _TerminalAppName,
            _TerminalAppDirName,
            _TerminalAppExeName);

      Assert.Throws<ArgumentNullException>(()=>projectInfo.CreateDeploymentTask(null));
    }

    [Test]
    public void Test_CreateDeployemntTask_RunsProperly_WhenAllIsWell()
    {
      var objectFactory = new Mock<IObjectFactory>(MockBehavior.Strict);
      var envInfoRepository = new Mock<IEnvironmentInfoRepository>(MockBehavior.Strict);
      var artifactsRepository = new Mock<IArtifactsRepository>(MockBehavior.Strict);
      var taskScheduler = new Mock<ITaskScheduler>(MockBehavior.Strict);
      var ntServiceManager = new Mock<INtServiceManager>(MockBehavior.Strict);

      var projectInfo = new TerminalAppProjectInfo(
            _Name,
            _ArtifactsRepositoryName,
            _ArtifactsRepositoryDirName,
            _ArtifactsAreNotEnvirionmentSpecific,
            _TerminalAppName,
            _TerminalAppDirName,
            _TerminalAppExeName);

      objectFactory.Setup(o => o.CreateEnvironmentInfoRepository()).Returns(envInfoRepository.Object);
      objectFactory.Setup(o => o.CreateArtifactsRepository()).Returns(artifactsRepository.Object);
      objectFactory.Setup(o => o.CreateTaskScheduler()).Returns(taskScheduler.Object);
      objectFactory.Setup(o => o.CreateNtServiceManager()).Returns(ntServiceManager.Object);

      projectInfo.CreateDeploymentTask(objectFactory.Object);
    }

    [Test]
    public void Test_GetTargetFolders_RunsProperly_WhenAllIsWell()
    {
      string machine = Environment.MachineName;
      const string baseDirPath = "c:\\basedir";
      string terminalmachine = "terminalmachine";
      
      var envInfo =
        new EnvironmentInfo(
          "name",
          "templates",
          machine,
          "failover",
          new[] { "webmachine" },
          terminalmachine,
          "databasemachine",
          baseDirPath,
          "webbasedir",
          "c:\\scheduler",
          "c:\\terminal",
          false,
          _EnvironmentUsers,
          _ProjectToFailoverClusterGroupMappings);

      var projectInfo = new TerminalAppProjectInfo(
            _Name,
            _ArtifactsRepositoryName,
            _ArtifactsRepositoryDirName,
            _ArtifactsAreNotEnvirionmentSpecific,
            _TerminalAppName,
            _TerminalAppDirName,
            _TerminalAppExeName);

      List<string> targetFolders =
              projectInfo.GetTargetFolders(envInfo)
                .ToList();

      Assert.IsNotNull(targetFolders);
      Assert.AreEqual(1, targetFolders.Count);
      Assert.AreEqual("\\\\" + terminalmachine + "\\c$\\terminal\\" + _TerminalAppDirName, targetFolders[0]);
    }

    [Test]
    public void Test_GetTargetFolders_Throws_EnvInfo_null()
    {
      var projectInfo = new TerminalAppProjectInfo(
            _Name,
            _ArtifactsRepositoryName,
            _ArtifactsRepositoryDirName,
            _ArtifactsAreNotEnvirionmentSpecific,
            _TerminalAppName,
            _TerminalAppDirName,
            _TerminalAppExeName);

      Assert.Throws<ArgumentNullException>(() => projectInfo.GetTargetFolders(null));
    }
  }
}
