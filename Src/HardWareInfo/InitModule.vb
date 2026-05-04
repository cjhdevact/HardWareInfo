Imports System.Runtime.InteropServices

Module InitModule
    Public ConsMode As Integer
    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function AllocConsole() As Boolean
    End Function

    <DllImport("kernel32.dll")>
    Private Function GetConsoleWindow() As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Function SetWindowText(ByVal hWnd As IntPtr, ByVal lpString As String) As Boolean
    End Function
    Public Property ConsoleHWnd As IntPtr
    Sub Main()
        If Command.ToLower.Contains("/c") Then
            If ConsoleHWnd = IntPtr.Zero Then
                AllocConsole()
            End If
            ConsMode = 1
            ConsoleHWnd = GetConsoleWindow()
            SetWindowText(ConsoleHWnd, "系统硬件信息读取工具 版本 " & My.Application.Info.Version.ToString)
            Console.OutputEncoding = System.Text.Encoding.UTF8
            Console.Title = "系统硬件信息读取工具 版本 " & My.Application.Info.Version.ToString
            InfoReadForm.StartDetection()
            Console.ReadKey()
        Else
            ConsMode = 0
            InfoReadForm.ShowDialog()
        End If
    End Sub
End Module
