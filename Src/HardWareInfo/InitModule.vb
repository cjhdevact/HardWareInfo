'****************************************************************************
'    HardWareInfo
'    Copyright (C) 2026 CJH
'
'    This program is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    This program is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with this program.  If not, see <http://www.gnu.org/licenses/>.
'****************************************************************************
'/*****************************************************\
'*                                                     *
'*     HardWareInfo - InitModule.vb                    *
'*                                                     *
'*     Copyright (c) CJH.                              *
'*                                                     *
'*     The program startup module.                     *
'*                                                     *
'\*****************************************************/
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
            SetWindowText(ConsoleHWnd, "系统信息硬件记录读取工具 版本 " & My.Application.Info.Version.ToString)
            Console.OutputEncoding = System.Text.Encoding.UTF8
            Console.Title = "系统信息硬件记录读取工具 版本 " & My.Application.Info.Version.ToString
            InfoReadForm.StartDetection()
            Console.ReadKey()
        Else
            ConsMode = 0
            InfoReadForm.ShowDialog()
        End If
    End Sub
End Module
