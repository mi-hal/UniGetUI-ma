﻿using Microsoft.UI.Xaml;
using ModernWindow.Structures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ModernWindow.PackageEngine
{
    
    public class __serializable_exportable_packages
    {
        public double export_version { get; set; } = 2.0;
        public List<__serializable_bundled_package_v1> packages { get; set; } = new();
        public string incompatible_packages_info { get; set; } = "Incompatible packages cannot be installed from WingetUI, but they have been listed here for logging purposes.";
        public List<__serializable_incompatible_package_v1> incompatible_packages { get; set; } = new();

    }
    public enum DeserializedPackageStatus
    {
        ManagerNotFound,
        ManagerNotEnabled,
        ManagerNotReady,
        SourceNotFound,
        IsAvailable
    }
    public class __serializable_updates_options_v1
    {
        public bool UpdatesIgnored { get; set; } = false;
        public string IgnoredVersion { get; set; } = "";
        public static async Task<__serializable_updates_options_v1> FromPackage(Package package)
        {
            var Serializable = new __serializable_updates_options_v1();
            Serializable.UpdatesIgnored = await package.HasUpdatesIgnored();
            Serializable.IgnoredVersion = await package.GetIgnoredUpdatesVersion();
            return Serializable;
        }
    }
    public class __serializable_bundled_package_v1
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Version { get; set; } = "";
        public string Source { get; set; } = "";
        public string ManagerName { get; set; } = "";
        public InstallationOptions.__serializable_options_v1 InstallationOptions { get; set; }
        public __serializable_updates_options_v1 Updates { get; set; }
    }

    public class __serializable_incompatible_package_v1
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Version { get; set; } = "";
        public string Source { get; set; } = "";
    }



    public enum BundleFormatType
    {
        JSON,
        YAML,
        XML
    }

    public class BundledPackage : INotifyPropertyChanged
    {
        

       

        public AppTools bindings = AppTools.Instance;
        public Package Package { get; }
        public bool IsValid { get; set; } = true;
        bool RequiresImportWhenInstalling { get; } = false;
        public InstallationOptions Options { get; set; }
        public __serializable_updates_options_v1 UpdateOptions = null;

        public double DrawOpacity = 1;

        public event PropertyChangedEventHandler PropertyChanged;

        private bool __is_checked = true;
        public bool IsChecked { get { return __is_checked; } set { __is_checked = value; OnPropertyChanged(); } }


        public virtual string Name
        {
            get
            {
                return Package.Name;
            }
        }

        public virtual string Id
        {
            get
            {
                return Package.Id;
            }
        }

        public virtual string version
        {
            get
            {
                if (UpdateOptions == null || !UpdateOptions.UpdatesIgnored)
                    return bindings.Translate("Latest");
                else
                    return Package.Version;
            }
        }

        public virtual string SourceAsString
        {
            get
            {
                return Package.SourceAsString;
            }
        }

        public virtual string IconId
        {
            get
            {
                return Package.Source.IconId;
            }
        }
        public BundledPackage(Package package)
        {
            Package = package;
            Options = new InstallationOptions(package);
            RequiresImportWhenInstalling = false;
        }

        public BundledPackage(Package package, InstallationOptions options, __serializable_updates_options_v1 updateOptions)
        {
            Package = package;
            RequiresImportWhenInstalling = true;
            Options = options;
            UpdateOptions = updateOptions;
        }

        public async virtual void ShowOptions(object sender, RoutedEventArgs e)
        {
            Options = await bindings.App.mainWindow.NavigationPage.UpdateInstallationSettings(Package, Options);
        }

        public void RemoveFromList(object sender, RoutedEventArgs e)
        {
            bindings.App.mainWindow.NavigationPage.BundlesPage.Packages.Remove(this);
            bindings.App.mainWindow.NavigationPage.BundlesPage.FilteredPackages.Remove(this);
            bindings.App.mainWindow.NavigationPage.BundlesPage.UpdateCount();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }

    public class InvalidBundledPackage : BundledPackage
    {
        string __name;
        string __id;
        string __version;
        string __source;
        string __manager;

        public override string Name
        {
            get
            {
                return __name;
            }
        }

        public override string Id
        {
            get
            {
                return __id;
            }
        }

        public override string version
        {
            get
            {
                return __version;
            }
        }

        public override string SourceAsString
        {
            get
            {
                if (__source == "")
                    return __manager;
                return __manager + ": " + __source;
            }
        }

        public override string IconId
        {
            get
            {
                return "help";
            }
        }

        public InvalidBundledPackage(string name, string id, string version, string source, string manager) : base(new Package(name, id, version, AppTools.Instance.App.Winget.MainSource, AppTools.Instance.App.Winget))
        {
            IsValid = false;
            DrawOpacity = 0.5;
            __name = name;
            __id = id;
            __version = version;
            __source = source;
            __manager = manager;
        }
        public InvalidBundledPackage(Package package) : base(package)
        {
            IsValid = false;
            DrawOpacity = 0.5;
            __name = package.Name;
            __id = package.Id;
            __version = package.Version;
            if (!package.Manager.Capabilities.SupportsCustomSources)
            {
                __source = "";
                __manager = package.Manager.Name;
            }
            else if (package.Source.IsVirtualManager)
            {
                __source = "";
                __manager = package.Source.Name;
            }
            else
            {
                __source = package.Source.Name;
                __manager = package.Manager.Name;
            }
        }
        public async override void ShowOptions(object sender, RoutedEventArgs e)
        {
            // Todo: show warning?
        }

    }

}
