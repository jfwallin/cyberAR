using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

namespace Tests
{
    public class ProjectSetup
    {
        [Test]
        public void build_target_is_set_to_lumin()
        {
            Assert.AreEqual(BuildTarget.Lumin, EditorUserBuildSettings.activeBuildTarget);
        }

        [Test]
        public void project_is_set_to_development_build()
        {
            Assert.IsTrue(EditorUserBuildSettings.development);
        }

        [Test]
        public void at_least_one_scene_included_in_build()
        {
            Assert.Greater(EditorBuildSettings.scenes.Length, 0);
        }

        [Test]
        public void lumin_publisher_settings_sign_package_set_to_true()
        {
            Assert.IsTrue(PlayerSettings.Lumin.signPackage);
        }

        [Test]
        public void mlcertificate_path_is_set()
        {
            Assert.IsTrue(PlayerSettings.Lumin.certificatePath.EndsWith(".cert"));
        }

        //INCORRECT - as much as possible, you should test only one thing in a test
        //[Test]
        //public void mlcertificate_is_set_up_correctly()
        //{
        //    Assert.IsTrue((PlayerSettings.Lumin.signPackage == true) && PlayerSettings.Lumin.certificatePath.EndsWith(".cert"));
        //}
    }
}
