# Arc Browser Starter

### EN:
<details>
This application fixes the problem when Arc Browser does not start after authorization.<br><br>

Problem sequence:
1. Install the application.
2. Authorization in the application.
3. Close the application.
4. Attempt to start the application.
5. The application does not start.

All this happens because of the folder `C:\Users\%username%\AppData\Local\Packages\TheBrowserCompany.Arc_{ArcBrowserMaybeId}\LocalCache\Local\firestore\Arc`, which will have to be deleted every time the application is started.
This application deletes this folder and starts Arc Browser.

I hope this application will help you and you will be able to continue using Arc Browser without any problems)
</details>

### RU:
<details>
Данное приложение исправляет проблему, когда Arc Browser не запускается после авторизации.<br><br>

Последовательность проблемы:
1. Установка приложения.
2. Авторизация в приложении.
3. Закрытие приложения.
4. Попытка запуска приложения.
5. Приложение не запускается.

Всё это происходит из-за папки `C:\Users\%username%\AppData\Local\Packages\TheBrowserCompany.Arc_{ArcBrowserMaybeId}\LocalCache\Local\firestore\Arc`, которую придется удалить каждый раз при запуске приложения.
Данное приложение удаляет эту папку и запускает Arc Browser.

Надеюсь, что данное приложение вам поможет и вы сможете как и я дальше пользоваться Arc Browser без каких-либо проблем)
</details>

Download/Загрузить: [Releases](https://github.com/KataLoved/ArcBrowserStarter/releases)

<p align="right">
	<img src="https://badges.pufler.dev/visits/KataLoved/ArcBrowserStarter?color=black&logo=github"/>
</p>
