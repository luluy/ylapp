namespace Jihua
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.ylJIhuaServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ylJihuaserviceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ylJIhuaServiceProcessInstaller
            // 
            this.ylJIhuaServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ylJIhuaServiceProcessInstaller.Password = null;
            this.ylJIhuaServiceProcessInstaller.Username = null;
            this.ylJIhuaServiceProcessInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstaller1_AfterInstall);
            // 
            // ylJihuaserviceInstaller
            // 
            this.ylJihuaserviceInstaller.ServiceName = "ylJIhuaService";
            this.ylJihuaserviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.ylJihuaserviceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceInstaller1_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ylJIhuaServiceProcessInstaller,
            this.ylJihuaserviceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller ylJIhuaServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller ylJihuaserviceInstaller;
    }
}