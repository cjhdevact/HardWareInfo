Imports Microsoft.Win32
Imports System.Collections.Generic

Public Class InfoReadForm

    ' 中断标志和线程
    Private cancelDetection As Boolean = False
    Private CheckProgress As System.Threading.Thread = Nothing

    ' 当前注册表根路径（默认 LocalMachine，后续可改为外部挂载的注册表根路径）
    Private ReadOnly Property RegistryRoot As RegistryKey
        Get
            Return Registry.LocalMachine
        End Get
    End Property
    Private ReadOnly Property RegistryRootUser As RegistryKey
        Get
            Return Registry.CurrentUser
        End Get
    End Property

    ' 向文本框追加内容的方法
    Private Sub AppendToTextBox(ByVal text As String)
        If ConsMode = 0 Then
            If TextBox1.InvokeRequired Then
                TextBox1.Invoke(Sub() AppendToTextBox(text))
            Else
                TextBox1.AppendText(text & Environment.NewLine)
            End If
        Else
            Console.WriteLine(text)
        End If
    End Sub

    ' 按钮点击事件 - 开始检测
    Private Sub Button1_Click(ByVal sender As Object, ByVal e As EventArgs) Handles Button1.Click
        ' 如果已经在运行，则提示
        If CheckProgress IsNot Nothing AndAlso CheckProgress.IsAlive Then
            MessageBox.Show("检测正在进行中，请稍候或点击中断按钮", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' 清空文本框
        TextBox1.Clear()
        TextBox1.Enabled = False
        Button1.Enabled = False
        Button2.Enabled = False
        Button3.Enabled = False
        Button4.Enabled = True
        Button4.Visible = True
        ' 重置中断标志
        cancelDetection = False

        ' 在新线程中执行检测
        CheckProgress = New System.Threading.Thread(AddressOf StartDetection)
        CheckProgress.IsBackground = True
        CheckProgress.Start()
    End Sub

    ' 检测主流程（在新线程中运行）
    Public Sub StartDetection()
        Try
            ' 显示开始信息
            AppendToTextBox("系统信息硬件记录读取工具 版本 " & My.Application.Info.Version.ToString)
            AppendToTextBox("版权所有 © 2026 CJH。保留所有权利。")

            ' Windows 系统信息
            If CheckBox1.Checked = True Then
                If Not cancelDetection Then ShowWindowsInfo()
            End If


            ' 已安装软件
            If CheckBox2.Checked = True Then
                If Not cancelDetection Then ShowInstalledSoftware()
            End If


            ' CPU 处理器信息
            If CheckBox3.Checked = True Then
                If Not cancelDetection Then ShowCpuInfo()
            End If


            ' 显卡 GPU 信息
            If CheckBox4.Checked = True Then
                If Not cancelDetection Then ShowGpuInfo()
            End If


            ' 硬盘信息
            If CheckBox5.Checked = True Then
                If Not cancelDetection Then ShowDiskInfo()
            End If


            ' 主板 / BIOS 信息
            If CheckBox6.Checked = True Then
                If Not cancelDetection Then ShowMotherboardBiosInfo()
            End If


            ' 网络适配器信息
            If CheckBox7.Checked = True Then
                If Not cancelDetection Then ShowNetworkAdapterInfo()
            End If

            ' 音频设备
            If CheckBox9.Checked = True Then
                If Not cancelDetection Then ShowAudioDeviceInfo()
            End If

            If Not cancelDetection Then
                AppendToTextBox("")
                AppendToTextBox("[INFO] 硬件信息读取完成")
            Else
                AppendToTextBox("")
                AppendToTextBox("[INFO] 硬件信息读取已中断")
            End If
        Catch ex As Threading.ThreadAbortException
            AppendToTextBox("")
            AppendToTextBox("[INFO] 硬件信息读取已强制中断")
        Catch ex As Exception
            AppendToTextBox("")
            AppendToTextBox("[ERROR] 检测过程发生错误：" & ex.Message & " =====")
        Finally
            ' 恢复按钮状态
            If TextBox1.InvokeRequired Then
                TextBox1.Invoke(Sub() RestoreButtons())
            Else
                RestoreButtons()
            End If
        End Try
    End Sub

    ' 恢复按钮状态
    Private Sub RestoreButtons()
        TextBox1.Enabled = True
        Button1.Enabled = True
        Button2.Enabled = True
        Button3.Enabled = True
        Button4.Enabled = False
        Button4.Visible = False
    End Sub

    ' 按钮4点击事件 - 中断检测
    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        cancelDetection = True

        ' 强制终止检测线程
        If CheckProgress IsNot Nothing AndAlso CheckProgress.IsAlive Then
            Try
                CheckProgress.Abort()
            Catch
                ' 忽略线程终止时的异常
            End Try
        End If

        AppendToTextBox("")
        AppendToTextBox("[INFO] 用户已请求中断检测")
    End Sub

#Region "Windows 系统信息"
    Sub ShowWindowsInfo()
        If cancelDetection Then Exit Sub

        AppendToTextBox("")
        AppendToTextBox("[ Windows 系统信息 ]")
        Try
            Dim reg As RegistryKey = RegistryRoot.OpenSubKey("SOFTWARE\Microsoft\Windows NT\CurrentVersion")
            If reg IsNot Nothing Then
                AppendToTextBox("  系统名称    : " & GetRegValue(reg, "ProductName", "未知"))
                AppendToTextBox("  版本号      : " & GetRegValue(reg, "DisplayVersion", "未知"))
                AppendToTextBox("  发布ID      : " & GetRegValue(reg, "ReleaseId", "未知"))
                AppendToTextBox("  Build 号    : " & GetRegValue(reg, "CurrentBuild", "未知"))
                AppendToTextBox("  UBR         : " & GetRegValue(reg, "UBR", "未知"))
                AppendToTextBox("  Build 分支  : " & GetRegValue(reg, "BuildBranch", "未知"))
                AppendToTextBox("  注册所有者  : " & GetRegValue(reg, "RegisteredOwner", "未知"))
                AppendToTextBox("  注册组织    : " & GetRegValue(reg, "RegisteredOrganization", "未知"))
                AppendToTextBox("  安装类型    : " & GetRegValue(reg, "InstallationType", "未知"))
                AppendToTextBox("  版本ID      : " & GetRegValue(reg, "EditionID", "未知"))
                reg.Close()
            End If
        Catch ex As Exception
            AppendToTextBox("  读取失败：" & ex.Message)
        End Try
    End Sub
#End Region

#Region "已安装软件"
    Sub ShowInstalledSoftware()
        If cancelDetection Then Exit Sub

        AppendToTextBox("")
        AppendToTextBox("[ 已安装软件 ]")
        Try
            Dim keys As New List(Of String)
            keys.Add("SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")
            keys.Add("SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall")

            Dim count As Integer = 0
            For Each k In keys
                If cancelDetection Then Exit Sub

                Dim uKey As RegistryKey = RegistryRoot.OpenSubKey(k)
                If uKey IsNot Nothing Then
                    For Each Name2 In uKey.GetSubKeyNames()
                        If cancelDetection Then Exit Sub

                        Dim subKey As RegistryKey = uKey.OpenSubKey(Name2)
                        Dim dispName As String = GetRegValue(subKey, "DisplayName", "")
                        Dim dispVer As String = GetRegValue(subKey, "DisplayVersion", "")
                        If dispName <> "" Then
                            AppendToTextBox("  " & dispName & " | " & dispVer)
                            count += 1
                        End If
                        subKey.Close()
                    Next
                    uKey.Close()
                End If
            Next

            Dim keys1 As New List(Of String)
            keys1.Add("SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")
            For Each k In keys1
                If cancelDetection Then Exit Sub

                Dim uKey As RegistryKey = RegistryRootUser.OpenSubKey(k)
                If uKey IsNot Nothing Then
                    For Each Name1 In uKey.GetSubKeyNames()
                        If cancelDetection Then Exit Sub

                        Dim subKey As RegistryKey = uKey.OpenSubKey(Name1)
                        Dim dispName As String = GetRegValue(subKey, "DisplayName", "")
                        Dim dispVer As String = GetRegValue(subKey, "DisplayVersion", "")
                        If dispName <> "" Then
                            AppendToTextBox("  " & dispName & " | " & dispVer)
                            count += 1
                        End If
                        subKey.Close()
                    Next
                    uKey.Close()
                End If
            Next

            If count = 0 Then AppendToTextBox("  未找到已安装软件")
        Catch ex As Exception
            AppendToTextBox("  读取失败：" & ex.Message)
        End Try
    End Sub
#End Region

#Region "CPU 处理器信息"
    Sub ShowCpuInfo()
        If cancelDetection Then
            Exit Sub
        End If


        AppendToTextBox("")
        AppendToTextBox("[ CPU 处理器信息 ]")
        Try
            Dim cpuList As New HashSet(Of String)()
            Dim cpuDetailList As New List(Of String)()
            Dim CPU_CLASS_GUID As String = "{50127dc3-0f36-415e-a6cc-4cb3be910b65}"

            ' 只扫描 ACPI 路径（按你的要求）
            Dim acpiKey As RegistryKey = RegistryRoot.OpenSubKey("SYSTEM\CurrentControlSet\Enum\ACPI")
            If acpiKey IsNot Nothing Then
                For Each keyName As String In acpiKey.GetSubKeyNames()
                    If cancelDetection Then
                        Exit Sub
                    End If

                    Dim vendorKey As RegistryKey = acpiKey.OpenSubKey(keyName)
                    If vendorKey IsNot Nothing Then
                        For Each instName As String In vendorKey.GetSubKeyNames()
                            If cancelDetection Then
                                Exit Sub
                            End If

                            Dim cpuKey As RegistryKey = Nothing
                            Try
                                ' 安全打开，无子键自动跳过
                                cpuKey = vendorKey.OpenSubKey(instName)
                                If cpuKey Is Nothing Then Continue For

                                ' 唯一判断：ClassGUID 匹配就是CPU（无任何硬编码）
                                Dim classGuid As String = GetRegValue(cpuKey, "ClassGUID", "").Trim()
                                If classGuid.Equals(CPU_CLASS_GUID, StringComparison.OrdinalIgnoreCase) Then

                                    Dim cpuName As String = GetRegValue(cpuKey, "FriendlyName", "").Trim()
                                    If String.IsNullOrWhiteSpace(cpuName) Then
                                        cpuName = GetRegValue(cpuKey, "DeviceDesc", "未知处理器").Trim()
                                    End If

                                    If Not cpuList.Contains(cpuName) Then
                                        cpuList.Add(cpuName)


                                        ' 输出格式保持和你原来完全一致
                                        Dim info As String = "  " & cpuName & Environment.NewLine

                                        cpuDetailList.Add(info)
                                    End If
                                End If

                            Catch
                                ' 异常直接跳过，不崩溃
                                Continue For
                            Finally
                                If cpuKey IsNot Nothing Then cpuKey.Close()
                            End Try
                        Next
                        vendorKey.Close()
                    End If
                Next
                acpiKey.Close()
            End If

            ' 输出结果
            If cpuDetailList.Count > 0 Then
                For Each info In cpuDetailList
                    If cancelDetection Then
                        Exit Sub
                    End If
                    AppendToTextBox(info)
                Next
            Else
                ' 备用方案（兜底）
                Dim cpu0Key As RegistryKey = RegistryRoot.OpenSubKey("HARDWARE\DESCRIPTION\System\CentralProcessor\0")
                If cpu0Key IsNot Nothing Then
                    Dim procName As String = GetRegValue(cpu0Key, "ProcessorNameString", "未知CPU")
                    Dim vendor As String = GetRegValue(cpu0Key, "VendorIdentifier", "未知")
                    Dim mhz As String = GetRegValue(cpu0Key, "~MHz", "未知")

                    Dim coreCount As Integer = 0
                    Dim coreKey As RegistryKey = RegistryRoot.OpenSubKey("HARDWARE\DESCRIPTION\System\CentralProcessor")
                    If coreKey IsNot Nothing Then
                        coreCount = coreKey.GetSubKeyNames().Length
                        coreKey.Close()
                    End If

                    AppendToTextBox("  [当前CPU信息] " & procName)
                    AppendToTextBox("    最大频率   : " & mhz & " MHz")
                    AppendToTextBox("    厂商       : " & vendor)
                    AppendToTextBox("    逻辑核心数 : " & coreCount)
                    cpu0Key.Close()
                End If
            End If

        Catch ex As Exception
            AppendToTextBox("  读取失败：" & ex.Message)
        End Try
    End Sub
#End Region

#Region "显卡 GPU 信息"
    Sub ShowGpuInfo()
        If cancelDetection Then Exit Sub

        AppendToTextBox("")
        AppendToTextBox("[ 显卡 GPU 信息 ]")
        Try
            Dim displayKey As RegistryKey = RegistryRoot.OpenSubKey("SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}")
            If displayKey IsNot Nothing Then
                For Each subName In displayKey.GetSubKeyNames()
                    If cancelDetection Then Exit Sub

                    ' 忽略系统文件夹 Configuration 和 Properties，只读取显卡设备项
                    If subName = "Configuration" OrElse subName = "Properties" Then
                        Continue For
                    End If

                    Dim gpuKey As RegistryKey = displayKey.OpenSubKey(subName)
                    Dim desc As String = GetRegValue(gpuKey, "DriverDesc", "")
                    Dim provider As String = GetRegValue(gpuKey, "ProviderName", "")

                    If CheckBox8.Checked = True Then
                        ' 过滤微软自带虚拟显卡
                        If provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase) Then
                            gpuKey.Close()
                            Continue For
                        End If
                    End If

                    If desc <> "" Then
                        AppendToTextBox("  " & desc)
                        Dim drvVer As String = GetRegValue(gpuKey, "DriverVersion", "")
                        If drvVer <> "" Then AppendToTextBox("    驱动版本   : " & drvVer)
                        Dim drvDate As String = GetRegValue(gpuKey, "DriverDate", "")
                        If drvDate <> "" Then AppendToTextBox("    驱动日期   : " & drvDate)
                    End If
                    gpuKey.Close()
                Next
                displayKey.Close()
            End If

            ' 备用：从 PCI 枚举显卡（用于离线场景）
            Dim pciKey As RegistryKey = RegistryRoot.OpenSubKey("SYSTEM\CurrentControlSet\Enum\PCI")
            If pciKey IsNot Nothing Then
                For Each ven In pciKey.GetSubKeyNames()
                    If cancelDetection Then Exit Sub

                    For Each dev In pciKey.GetSubKeyNames(ven)
                        If cancelDetection Then Exit Sub

                        Dim devKey As RegistryKey = pciKey.OpenSubKey(ven & "\" & dev)
                        Dim classGuid As String = GetRegValue(devKey, "Class", "")
                        If classGuid = "Display" OrElse classGuid = "{4d36e968-e325-11ce-bfc1-08002be10318}" Then
                            Dim desc As String = GetRegValue(devKey, "DeviceDesc", "")
                            If desc <> "" Then
                                AppendToTextBox("  [PCI] " & desc)
                            End If
                        End If
                        devKey.Close()
                    Next
                Next
                pciKey.Close()
            End If
        Catch ex As Exception
            AppendToTextBox("  读取失败：" & ex.Message)
        End Try
    End Sub
#End Region

#Region "硬盘信息"
    Sub ShowDiskInfo()
        If cancelDetection Then Exit Sub

        AppendToTextBox("")
        AppendToTextBox("[ 硬盘信息 ]")
        Try
            Dim diskPaths As New List(Of String) From {
                "SYSTEM\CurrentControlSet\Enum\SCSI",
                "SYSTEM\CurrentControlSet\Enum\USBSTOR",
                "SYSTEM\CurrentControlSet\Enum\IDE",
                "SYSTEM\CurrentControlSet\Enum\PCI\VEN_8086",
                "SYSTEM\CurrentControlSet\Enum\STORAGE"
            }

            Dim found As Boolean = False
            For Each diskPath In diskPaths
                If cancelDetection Then Exit Sub

                Dim key As RegistryKey = RegistryRoot.OpenSubKey(diskPath)
                If key IsNot Nothing Then
                    For Each sub1 In key.GetSubKeyNames()
                        If cancelDetection Then Exit Sub

                        Dim sub1Key As RegistryKey = key.OpenSubKey(sub1)
                        If sub1Key IsNot Nothing Then
                            For Each sub2 In sub1Key.GetSubKeyNames()
                                If cancelDetection Then Exit Sub

                                Dim devKey As RegistryKey = sub1Key.OpenSubKey(sub2)
                                Dim name As String = GetRegValue(devKey, "FriendlyName", "")
                                If name = "" Then name = GetRegValue(devKey, "Product", "")
                                If name = "" Then name = GetRegValue(devKey, "DeviceDesc", "")

                                If name <> "" AndAlso Not name.Contains("Volume") Then
                                    AppendToTextBox("  [" & IO.Path.GetFileName(diskPath) & "] " & name)
                                    found = True

                                    ' 读取序列号
                                    Dim serial As String = GetRegValue(devKey, "SerialNumber", "")
                                    If serial <> "" Then AppendToTextBox("    序列号   : " & serial)

                                    ' 读取固件版本
                                    Dim fw As String = GetRegValue(devKey, "FirmwareRevision", "")
                                    If fw <> "" Then AppendToTextBox("    固件版本 : " & fw)
                                End If
                                devKey.Close()
                            Next
                            sub1Key.Close()
                        End If
                    Next
                    key.Close()
                End If
            Next

            If Not found Then AppendToTextBox("  未找到硬盘信息")
        Catch ex As Exception
            AppendToTextBox("  读取失败：" & ex.Message)
        End Try
    End Sub
#End Region

#Region "主板 / BIOS 信息"
    Sub ShowMotherboardBiosInfo()
        If cancelDetection Then Exit Sub

        AppendToTextBox("")
        AppendToTextBox("[ 主板 / BIOS 信息 ]")
        Try
            Dim hwConfigPath As String = "SYSTEM\HardwareConfig"
            Dim hwConfigKey As RegistryKey = RegistryRoot.OpenSubKey(hwConfigPath)

            If hwConfigKey IsNot Nothing Then
                Dim biosIds As String() = hwConfigKey.GetSubKeyNames()
                Dim index As Integer = 1

                For Each biosId As String In biosIds
                    If cancelDetection Then Exit Sub

                    ' 跳过名为 "Current" 的子项
                    If String.Equals(biosId, "Current", StringComparison.OrdinalIgnoreCase) Then
                        Continue For
                    End If

                    Dim subPath As String = hwConfigPath & "\" & biosId
                    Dim biosKey As RegistryKey = RegistryRoot.OpenSubKey(subPath)
                    If biosKey Is Nothing Then
                        Continue For
                    End If

                    ' 判断是否为最近使用（根据 LastUse 字段是否存在）
                    Dim lastUseValue As Object = biosKey.GetValue("LastUse", Nothing)
                    Dim isLastUsed As Boolean = (lastUseValue IsNot Nothing)

                    ' 输出编号 + BIOSID
                    Dim header As String = "  [" & index.ToString() & "] " & biosId
                    If isLastUsed Then
                        header &= " <- 最近使用"
                    End If
                    AppendToTextBox(header)

                    ' 从注册表读取正确的字段
                    Dim systemManufacturer As String = GetRegValue(biosKey, "SystemManufacturer", "未知")
                    Dim systemProductName As String = GetRegValue(biosKey, "SystemProductName", "未知")
                    Dim systemProductSku As String = GetRegValue(biosKey, "SystemSKU", "未知")
                    Dim baseBoardManufacturer As String = GetRegValue(biosKey, "BaseBoardManufacturer", "未知")
                    Dim baseBoardProduct As String = GetRegValue(biosKey, "BaseBoardProduct", "未知")
                    Dim biosVendor As String = GetRegValue(biosKey, "BIOSVendor", "未知")
                    Dim biosVersion As String = GetRegValue(biosKey, "BIOSVersion", "未知")
                    Dim biosReleaseDate As String = GetRegValue(biosKey, "BIOSReleaseDate", "未知")

                    ' 按格式输出
                    AppendToTextBox("      整机厂商      : " & systemManufacturer)
                    AppendToTextBox("      整机型号      : " & systemProductName)
                    AppendToTextBox("      整机SKU       : " & systemProductSku)
                    AppendToTextBox("      主板厂商      : " & baseBoardManufacturer)
                    AppendToTextBox("      主板型号      : " & baseBoardProduct)
                    AppendToTextBox("      BIOS 厂商     : " & biosVendor)
                    AppendToTextBox("      BIOS 版本     : " & biosVersion)
                    AppendToTextBox("      BIOS 日期     : " & biosReleaseDate)

                    biosKey.Close()
                    index = index + 1
                Next
                hwConfigKey.Close()
            End If
        Catch ex As Exception
            AppendToTextBox("  读取失败：" & ex.Message)
        End Try
    End Sub
#End Region

#Region "网络适配器信息"
    Sub ShowNetworkAdapterInfo()
        If cancelDetection Then Exit Sub

        AppendToTextBox("")
        AppendToTextBox("[ 网络适配器信息 ]")
        Try
            ' 通过网络适配器类 GUID 读取
            Dim netClassGuid As String = "{4d36e972-e325-11ce-bfc1-08002be10318}"
            Dim netKey As RegistryKey = RegistryRoot.OpenSubKey("SYSTEM\CurrentControlSet\Control\Class\" & netClassGuid)

            If netKey IsNot Nothing Then
                For Each subName In netKey.GetSubKeyNames()
                    If cancelDetection Then Exit Sub

                    ' 忽略系统文件夹 Configuration 和 Properties，只读取显卡设备项
                    If subName = "Configuration" OrElse subName = "Properties" Then
                        Continue For
                    End If

                    Dim adapterKey As RegistryKey = netKey.OpenSubKey(subName)
                    Dim desc As String = GetRegValue(adapterKey, "DriverDesc", "")
                    Dim netCfgId As String = GetRegValue(adapterKey, "NetCfgInstanceId", "")

                    ' 过滤：厂商是 Microsoft 跳过（系统自带网卡）
                    If CheckBox8.Checked = True Then
                        Dim provider As String = GetRegValue(adapterKey, "ProviderName", "")
                        If provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase) Then
                            adapterKey.Close()
                            Continue For
                        End If
                    End If

                    If desc <> "" Then
                        AppendToTextBox("  " & desc)

                        Dim drvVer As String = GetRegValue(adapterKey, "DriverVersion", "")
                        If drvVer <> "" Then AppendToTextBox("    驱动版本 : " & drvVer)

                        Dim drvDate As String = GetRegValue(adapterKey, "DriverDate", "")
                        If drvDate <> "" Then AppendToTextBox("    驱动日期 : " & drvDate)

                        ' 根据 NetCfgInstanceId 读取 MAC 地址
                        If netCfgId <> "" Then
                            Dim connKey As RegistryKey = RegistryRoot.OpenSubKey("SYSTEM\CurrentControlSet\Control\Network\{4d36e972-e325-11ce-bfc1-08002be10318}\" & netCfgId & "\Connection")
                            If connKey IsNot Nothing Then
                                Dim mac As String = GetRegValue(connKey, "PnpInstanceID", "")
                                If mac <> "" Then AppendToTextBox("    DeviceInstanceId: " & mac)
                                connKey.Close()
                            End If
                        End If
                        AppendToTextBox("")
                    End If
                    adapterKey.Close()

                Next
                netKey.Close()
            End If

            ' 备用：从枚举路径读取物理网卡
            Dim pnpKey As RegistryKey = RegistryRoot.OpenSubKey("SYSTEM\CurrentControlSet\Enum\PCI")
            If pnpKey IsNot Nothing Then
                For Each ven In pnpKey.GetSubKeyNames()
                    If cancelDetection Then Exit Sub

                    For Each dev In pnpKey.GetSubKeyNames(ven)
                        If cancelDetection Then Exit Sub

                        Dim devKey As RegistryKey = pnpKey.OpenSubKey(ven & "\" & dev)
                        Dim className As String = GetRegValue(devKey, "Class", "")
                        If className = "Net" Then
                            Dim desc As String = GetRegValue(devKey, "DeviceDesc", "")
                            If desc <> "" AndAlso Not desc.Contains("NDIS") Then
                                AppendToTextBox("  [PCI] " & desc)
                            End If
                        End If
                        devKey.Close()
                    Next
                Next
                pnpKey.Close()
            End If
        Catch ex As Exception
            AppendToTextBox("  读取失败：" & ex.Message)
        End Try
    End Sub
#End Region

#Region "音频设备信息"
    Sub ShowAudioDeviceInfo()
        If cancelDetection Then Exit Sub

        AppendToTextBox("")
        AppendToTextBox("[ 音频设备信息 ]")
        Try
            ' 先从音频类路径读取所有设备的完整驱动信息
            Dim audioClassGuid As String = "{4d36e96c-e325-11ce-bfc1-08002be10318}"
            Dim audioClassKey As RegistryKey = RegistryRoot.OpenSubKey("SYSTEM\CurrentControlSet\Control\Class\" & audioClassGuid)

            If audioClassKey IsNot Nothing Then
                For Each subName In audioClassKey.GetSubKeyNames()
                    If cancelDetection Then Exit Sub
                    If subName = "Configuration" OrElse subName = "Properties" Then Continue For

                    Dim deviceKey As RegistryKey = audioClassKey.OpenSubKey(subName)
                    Dim deviceDesc As String = GetRegValue(deviceKey, "DriverDesc", "")
                    Dim provider As String = GetRegValue(deviceKey, "ProviderName", "")
                    Dim drvVer As String = GetRegValue(deviceKey, "DriverVersion", "")
                    Dim drvDate As String = GetRegValue(deviceKey, "DriverDate", "")
                    Dim instanceId As String = GetRegValue(deviceKey, "MatchingDeviceId", "")

                    ' 过滤无效描述和微软虚拟设备
                    If CheckBox8.Checked AndAlso provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase) Then
                        deviceKey.Close()
                        Continue For
                    End If
                    If String.IsNullOrWhiteSpace(deviceDesc) OrElse deviceDesc.Contains("@oem") Then
                        deviceKey.Close()
                        Continue For
                    End If

                    AppendToTextBox("  " & deviceDesc)
                    AppendToTextBox("    驱动厂商   : " & provider)
                    AppendToTextBox("    硬件ID     : " & instanceId)
                    If Not String.IsNullOrWhiteSpace(drvVer) Then AppendToTextBox("    驱动版本   : " & drvVer)
                    If Not String.IsNullOrWhiteSpace(drvDate) Then AppendToTextBox("    驱动日期   : " & drvDate)
                    AppendToTextBox("")
                    deviceKey.Close()
                Next
                audioClassKey.Close()
            End If

            ' 读取USB音频设备（并补全驱动信息）
            Dim usbKey As RegistryKey = RegistryRoot.OpenSubKey("SYSTEM\CurrentControlSet\Enum\USB")
            If usbKey IsNot Nothing Then
                For Each ven In usbKey.GetSubKeyNames()
                    If cancelDetection Then Exit Sub

                    Dim venKey As RegistryKey = usbKey.OpenSubKey(ven)
                    If venKey Is Nothing Then Continue For

                    ' 遍历设备
                    For Each dev In venKey.GetSubKeyNames()
                        If cancelDetection Then Exit Sub

                        Dim devPath As String = ven & "\" & dev
                        Dim devKey As RegistryKey = venKey.OpenSubKey(dev)

                        Dim friendlyName As String = GetRegValue(devKey, "FriendlyName", "")
                        Dim devDesc As String = GetRegValue(devKey, "DeviceDesc", "")
                        Dim classGuid As String = GetRegValue(devKey, "ClassGUID", "")
                        Dim instanceId As String = GetRegValue(devKey, "HardwareID", "")

                        ' 只处理音频设备
                        If (classGuid = "{4d36e96c-e325-11ce-bfc1-08002be10318}" OrElse
                            devDesc.Contains("Audio") OrElse devDesc.Contains("Sound")) AndAlso
                            Not friendlyName.Contains("@oem") Then

                            Dim displayName As String = If(Not String.IsNullOrWhiteSpace(friendlyName), friendlyName, devDesc)
                            AppendToTextBox("  [USB音频] " & displayName)
                            AppendToTextBox("    硬件ID     : " & instanceId)
                            AppendToTextBox("")
                        End If
                        devKey.Close()
                    Next
                    venKey.Close()
                Next
                usbKey.Close()
            End If

            ' 读取HDAUDIO/HDMI显示器音频设备
            Dim hdaudioKey As RegistryKey = RegistryRoot.OpenSubKey("SYSTEM\CurrentControlSet\Enum\HDAUDIO")
            If hdaudioKey IsNot Nothing Then
                For Each funcId In hdaudioKey.GetSubKeyNames()
                    If cancelDetection Then Exit Sub
                    Dim funcKey As RegistryKey = hdaudioKey.OpenSubKey(funcId)
                    If funcKey IsNot Nothing Then
                        For Each devId In funcKey.GetSubKeyNames()
                            If cancelDetection Then Exit Sub
                            Dim devKey As RegistryKey = funcKey.OpenSubKey(devId)
                            If devKey IsNot Nothing Then
                                Dim devDesc As String = GetRegValue(devKey, "DeviceDesc", "")
                                Dim friendlyName As String = GetRegValue(devKey, "FriendlyName", "")
                                Dim instanceId As String = GetRegValue(devKey, "HardwareID", "")

                                If Not devDesc.Contains("@oem") AndAlso Not String.IsNullOrWhiteSpace(instanceId) Then
                                    Dim displayName As String = If(Not String.IsNullOrWhiteSpace(friendlyName), friendlyName, devDesc)
                                    AppendToTextBox("  [HDAUDIO] " & displayName)
                                    AppendToTextBox("    硬件ID     : " & instanceId)
                                    AppendToTextBox("")
                                End If
                                devKey.Close()
                            End If
                        Next
                        funcKey.Close()
                    End If
                Next
                hdaudioKey.Close()
            End If

            ' 备用：PCI音频设备
            Dim pciKey As RegistryKey = RegistryRoot.OpenSubKey("SYSTEM\CurrentControlSet\Enum\PCI")
            If pciKey IsNot Nothing Then
                For Each ven In pciKey.GetSubKeyNames()
                    If cancelDetection Then Exit Sub
                    Dim venKey As RegistryKey = pciKey.OpenSubKey(ven)
                    If venKey Is Nothing Then Continue For

                    For Each dev In venKey.GetSubKeyNames()
                        If cancelDetection Then Exit Sub
                        Dim devKey As RegistryKey = venKey.OpenSubKey(dev)
                        If devKey IsNot Nothing Then
                            Dim className As String = GetRegValue(devKey, "Class", "")
                            If className = "Media" Or className = "Audio" Then
                                Dim desc As String = GetRegValue(devKey, "DeviceDesc", "")
                                Dim hwid As String = GetRegValue(devKey, "HardwareID", "")
                                If desc <> "" AndAlso Not desc.Contains("@oem") Then
                                    AppendToTextBox("  [PCI音频设备] " & desc)
                                    AppendToTextBox("    硬件ID     : " & hwid)
                                    AppendToTextBox("")
                                End If
                            End If
                            devKey.Close()
                        End If
                    Next
                    venKey.Close()
                Next
                pciKey.Close()
            End If

        Catch ex As Exception
            AppendToTextBox("  读取失败：" & ex.Message)
        End Try
    End Sub
#End Region

#Region "通用读取函数"
    Function GetRegValue(ByVal key As RegistryKey, ByVal name As String, ByVal def As String) As String
        Try
            If key Is Nothing Then Return def
            Dim o As Object = key.GetValue(name, def)
            If o Is Nothing Then Return def

            ' 修复 REG_MULTI_SZ 读取报错（自动转为逗号分隔字符串）
            If TypeOf o Is String() Then
                Return String.Join(", ", CType(o, String()))
            End If

            Return o.ToString().Trim()
        Catch
            Return def
        End Try
    End Function

    ' 扩展方法：安全获取子键列表
    Function GetSubKeyNames(ByVal key As RegistryKey) As String()
        Try
            If key Is Nothing Then Return {}
            Return key.GetSubKeyNames()
        Catch
            Return {}
        End Try
    End Function

    Function GetSubKeyNames(ByVal key As RegistryKey, ByVal parentName As String) As String()
        Try
            Dim subKey As RegistryKey = key.OpenSubKey(parentName)
            If subKey Is Nothing Then Return {}
            Return subKey.GetSubKeyNames()
        Catch
            Return {}
        End Try
    End Function
#End Region

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Using sfd As New SaveFileDialog()
            ' 设置保存对话框参数
            sfd.Filter = "文本文件(*.txt)|*.txt|所有文件(*.*)|*.*"
            sfd.Title = "保存系统硬件信息"
            sfd.FileName = "系统信息硬件记录_" & My.Computer.Name.ToString & "_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".txt"
            sfd.DefaultExt = "txt"

            ' 如果用户点了保存
            If sfd.ShowDialog() = DialogResult.OK Then
                Try
                    ' 直接把 TextBox1 全部内容写入文件
                    System.IO.File.WriteAllText(sfd.FileName, TextBox1.Text, System.Text.Encoding.UTF8)
                    MessageBox.Show("保存成功！" & vbCrLf & "文件路径：" & sfd.FileName, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch ex As Exception
                    MessageBox.Show("保存失败：" & ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        TextBox1.Clear()
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        AboutForm.ShowDialog()
    End Sub
End Class