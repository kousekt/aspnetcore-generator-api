FROM microsoft/dotnet:2-sdk AS build-env
WORKDIR /integration

COPY integration.csproj .
RUN dotnet restore

# Copy in all of the source
COPY . .

# Defer execution of the tests to when you "RUN" the image (ie, docker run ...) rather
# then when you BUILD the image. So for that, we use ENTRYPOINT instead
ENTRYPOINT ["dotnet", "test"]
