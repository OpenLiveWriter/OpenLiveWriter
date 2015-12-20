# Connecting to Blogger From a Local Build

If you have built Open Live Writer from code, you may experience a problem if you try and connect to your Blogger blog, you might see this error:

![](https://cloud.githubusercontent.com/assets/3222168/11915551/e9f858ee-a670-11e5-9b07-d5e8f6aa8147.png)

If you do, then you will need to do the following in order to use Live Writer with your blog. 

1. Head over to the [Google developer console](https://console.developers.google.com/project)
2. Create a new project
    {Insert image here}
3. Once created, the console will take you to the dashboard for that project, click where it says Enable and Manage APIs
    {Insert image here}
4. Search for the Blogger API
    {Insert image here}
5. Enable that API
    {Insert image here}
6. Click on Credentials on the left
7. In the New Credentials drop down, choose OAuth Client ID
    {Insert image here}
8. If you haven't added any details for your project's consent screen, you'll need to do so at this point.
9. When prompted for your application type, choose other, and just give it a name, then click Create
10. You will then be shown the client ID and Client Secret that need to be used.
11. In your Live Writer repository, go to writer.build.targets and open it in a text editor.
12. Replace the ClientId and Client Secret details where it says PASTE\_YOUR\_CLIENT\_ID\_HERE and PASTE\_YOUR\_CLIENT\_SECRET\_HERE respectively
13. Now you should be able to build Live Writer again and connect your Blogger blog.