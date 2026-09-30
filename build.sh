Package_Name=nettag
Solution_Name=FileTagger.sln
Project_Path=FileTagger/FileTagger.csproj

dotnet build "$Solution_Name" -c Release

Package_Version=$(dotnet msbuild "$Project_Path" -getProperty:PackageVersion)
# Publish the project
dotnet publish "$Project_Path" -c Release -r linux-x64 -o "./publish/$Package_Name-$Package_Version"

# Remove the debug artifacts
rm "./publish/$Package_Name-$Package_Version"/*.pdb

#Rename the executable
mv "./publish/$Package_Name-$Package_Version/FileTagger" "./publish/$Package_Name-$Package_Version/$Package_Name"

# Copy the desktop file and icons
mkdir -p "./publish/$Package_Name-$Package_Version/icons/hicolor/scalable/apps/"
cp ./FileTagger/Assets/icon/tag-edit-blue.svg "./publish/$Package_Name-$Package_Version/icons/hicolor/scalable/apps/$Package_Name.svg"
chmod 644 "./publish/$Package_Name-$Package_Version/icons/hicolor/scalable/apps/$Package_Name.svg"
cp "$Package_Name.desktop" "./publish/$Package_Name-$Package_Version/"
chmod 644 "./publish/$Package_Name-$Package_Version/$Package_Name.desktop"

# Create the tarball
mkdir -p ./publish/tar/
cd ./publish/
tar -cvzf "./tar/$Package_Name-$Package_Version.tar.gz" "./$Package_Name-$Package_Version"
cd ..

# Increment the release number and set the version number in the spec file
RELEASE=$(awk '/^Release:/ { print $2 + 1; exit }' nettag.spec)
sed -i -E "s/^Version:.*/Version:    $Package_Version/; s/^Release:.*/Release:    $RELEASE/" nettag.spec

# Copy the tarball to the SOURCES directory and build the RPM
# Manually building these directories because the runner doesn't have the rpmbuild tools installed
mkdir -p ~/rpmbuild/SOURCES
mkdir -p ~/rpmbuild/BUILD
mkdir -p ~/rpmbuild/RPMS
mkdir -p ~/rpmbuild/SPECS
mkdir -p ~/rpmbuild/SRPMS
cp ./publish/tar/* ~/rpmbuild/SOURCES/
rpmbuild -bb nettag.spec

# Move the RPM to the publish directory
mkdir -p ./publish/rpm/
mv ~/rpmbuild/RPMS/x86_64/* ./publish/rpm/