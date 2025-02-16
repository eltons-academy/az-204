<%@ Import Namespace="System" %>
<%@ Page Language="c#"%>

<script runat="server">
    public string GetMachineName()
    {
        return Environment.MachineName;
    }
</script>

<html>

    <body>
        <div>
            <h1>Hello from <% =GetMachineName() %>!</h1>
        </div>
        <div class="footer">
            A demo app from the <a href="https://eltons.academy/az-204">Acing AZ-204 course</a> on <a href="https://eltons.academy">Elton's Academy</a>
        </div>
    </body>

</html>