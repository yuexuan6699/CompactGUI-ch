Imports CommunityToolkit.Mvvm.Input

Imports CompactGUI.Core.SharedMethods
Imports CompactGUI.Logging

Imports Microsoft.Extensions.Logging

Imports Wpf.Ui.Controls

Public Class CustomSnackBarService
    Inherits Wpf.Ui.SnackbarService

    Private ReadOnly logger As ILogger(Of CustomSnackBarService)
    Public _snackbar As Snackbar

    Public Sub New(logger As ILogger(Of CustomSnackBarService))
        MyBase.New()
        Me.logger = logger
    End Sub

    Public Sub ShowCustom(message As UIElement, title As String, appearance As ControlAppearance, Optional icon As IconElement = Nothing, Optional timeout As TimeSpan = Nothing)

        If GetSnackbarPresenter() Is Nothing Then Throw New InvalidOperationException("The SnackbarPresenter was never set")
        If _snackbar Is Nothing Then _snackbar = New Snackbar(GetSnackbarPresenter())

        _snackbar.SetCurrentValue(Snackbar.TitleProperty, title)
        _snackbar.SetCurrentValue(ContentControl.ContentProperty, message)
        _snackbar.SetCurrentValue(Snackbar.AppearanceProperty, appearance)
        _snackbar.SetCurrentValue(Snackbar.IconProperty, icon)
        _snackbar.SetCurrentValue(Snackbar.TimeoutProperty, If(timeout = Nothing, DefaultTimeOut, timeout))

        _snackbar.Show(True)
    End Sub



    Public Sub ShowInvalidFoldersMessage(InvalidFolders As List(Of String), InvalidMessages As List(Of FolderVerificationResult))

        Dim messageString = ""
        For i = 0 To InvalidFolders.Count - 1
            SnackbarServiceLog.ShowInvalidFoldersMessage(logger, InvalidFolders(i), GetFolderVerificationMessage(InvalidMessages(i)))
            If InvalidFolders.Count = 1 AndAlso InvalidMessages(i) = FolderVerificationResult.InsufficientPermission Then
                ShowInsufficientPermission(InvalidFolders(i))
                Return
            End If
            messageString &= $"{InvalidFolders(i)}: {GetFolderVerificationMessage(InvalidMessages(i))}" & vbCrLf
        Next

        Show("无效文件夹", messageString, Wpf.Ui.Controls.ControlAppearance.Danger, Nothing, TimeSpan.FromSeconds(10))

    End Sub

    Public Sub ShowInsufficientPermission(folderName As String)
        Dim button = New Button With {
            .Content = "以管理员身份重启",
            .Command = New RelayCommand(Sub() RunAsAdmin(folderName)),
            .Margin = New Thickness(-3, 10, 0, 0)
        }
        ShowCustom(button, "访问此文件夹的权限不足。", ControlAppearance.Danger, timeout:=TimeSpan.FromSeconds(60))
    End Sub

    Public Sub ShowUpdateAvailable(newVersion As String, isPreRelease As Boolean)
        Dim textBlock = New TextBlock
        textBlock.Text = "点击下载"

        ' Show the custom snackbar
        SnackbarServiceLog.ShowUpdateAvailable(logger, newVersion, isPreRelease)
        ShowCustom(textBlock, $"有可用更新 ▸ 版本 {newVersion}", If(isPreRelease, ControlAppearance.Info, ControlAppearance.Success), timeout:=TimeSpan.FromSeconds(10))

        Dim handler As MouseButtonEventHandler = Nothing
        Dim closedHandler As TypedEventHandler(Of Snackbar, RoutedEventArgs) = Nothing

        handler = Sub(sender, e)
                      Process.Start(New ProcessStartInfo("https://github.com/IridiumIO/CompactGUI/releases/") With {.UseShellExecute = True})
                      RemoveHandler Me.GetSnackbarPresenter.MouseDown, handler
                      RemoveHandler Me._snackbar.Closed, closedHandler
                  End Sub

        closedHandler = Sub(sender, e)
                            RemoveHandler Me.GetSnackbarPresenter.MouseDown, handler
                            RemoveHandler Me._snackbar.Closed, closedHandler
                        End Sub

        AddHandler Me.GetSnackbarPresenter.MouseDown, handler
        AddHandler Me._snackbar.Closed, closedHandler
    End Sub

    Public Sub ShowFailedToSubmitToWiki()
        Show("提交到 wiki 失败", "请检查您的网络连接后重试", Wpf.Ui.Controls.ControlAppearance.Danger, Nothing, TimeSpan.FromSeconds(5))
        SnackbarServiceLog.ShowFailedToSubmitToWiki(logger)
    End Sub

    Public Sub ShowSubmittedToWiki(steamsubmitdata As SteamSubmissionData, compressionMode As Integer)
        Show("已提交到 wiki", $"UID: {steamsubmitdata.UID}{vbCrLf}游戏: {steamsubmitdata.GameName}{vbCrLf}SteamID: {steamsubmitdata.SteamID}{vbCrLf}压缩: {[Enum].GetName(GetType(Core.WOFCompressionAlgorithm), Core.WOFHelper.WOFConvertCompressionLevel(compressionMode))}", Wpf.Ui.Controls.ControlAppearance.Success, Nothing, TimeSpan.FromSeconds(10))
        SnackbarServiceLog.ShowSubmittedToWiki(logger, steamsubmitdata.UID, steamsubmitdata.GameName, steamsubmitdata.SteamID, steamsubmitdata.CompressionMode)
    End Sub


    Public Sub ShowAppliedToAllFolders()
        Show("已应用到所有文件夹", "压缩选项已应用到所有文件夹", Wpf.Ui.Controls.ControlAppearance.Success, Nothing, TimeSpan.FromSeconds(5))
        SnackbarServiceLog.ShowAppliedToAllFolders(logger)
    End Sub

    Public Sub ShowCannotRemoveFolder()
        Show("无法移除文件夹", "请等待当前操作完成", Wpf.Ui.Controls.ControlAppearance.Caution, Nothing, TimeSpan.FromSeconds(5))
        SnackbarServiceLog.ShowCannotRemoveFolder(logger)
    End Sub

    Public Sub ShowAddedToQueue()
        Show("成功", "已添加到队列", Wpf.Ui.Controls.ControlAppearance.Success, Nothing, TimeSpan.FromSeconds(5))
        SnackbarServiceLog.ShowAddedToQueue(logger)
    End Sub

    Public Sub ShowDirectStorageWarning(displayName As String)
        Show(displayName,
            "此游戏使用 DirectStorage 技术。如果您正在使用此功能，则不应压缩此游戏。",
            Wpf.Ui.Controls.ControlAppearance.Info,
            Nothing,
            TimeSpan.FromSeconds(20))
        SnackbarServiceLog.ShowDirectStorageWarning(logger, displayName)
    End Sub
End Class