
FROM microsoft/aspnetcore:1.1.1
LABEL Name="webapi_aws_starter" Version="0.0.1"
COPY out /app
WORKDIR /app
EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT ["dotnet", "WebApi_AWS_Starter.dll"]
