### Build Stage - we will ONLY keep the runtime stage in the actual image

FROM microsoft/aspnetcore-build AS build-env

WORKDIR /generator

# restore - Test depends on the API
COPY api/api.csproj ./api/
RUN dotnet restore api/api.csproj

# Tests will most likely change more than api, so we put that later on in the
# dockerfile for optimization
COPY tests/tests.csproj ./tests/
RUN dotnet restore tests/tests.csproj

# recursively list all directorys.  Check which files are copied in.
# RUN ls -alR
#  docker run --rm testing ls -alR

# copy src
COPY . .

# run tests
# If the tests fail, we don't get an image

RUN dotnet test tests/tests.csproj

# publish the app

RUN dotnet publish api/api.csproj -o /publish


### Runtime stage Image - there will be no source code in this new image

FROM microsoft/aspnetcore
# copy that publish folder from our build stage
COPY --from=build-env /publish /publish

WORKDIR /publish
ENTRYPOINT ["dotnet", "api.dll"]