NAME=nettag
VERSION=0.1

dotnet publish FileTagger/FileTagger.csproj -c Release -r linux-x64 -o "./publish/$NAME-$VERSION"
rm "./publish/$NAME-$VERSION"/*.pdb
mv "./publish/$NAME-$VERSION/FileTagger" "./publish/$NAME-$VERSION/$NAME"
mkdir -p "./publish/$NAME-$VERSION/icons/hicolor/scalable/apps/"
cp ./FileTagger/Assets/icon/tag-edit-blue.svg "./publish/$NAME-$VERSION/icons/hicolor/scalable/apps/$NAME.svg"
chmod 644 "./publish/$NAME-$VERSION/icons/hicolor/scalable/apps/$NAME.svg"
cp "$NAME.desktop" "./publish/$NAME-$VERSION/"
chmod 644 "./publish/$NAME-$VERSION/$NAME.desktop"
mkdir -p ./publish/tar/
cd ./publish/
tar -cvzf "./tar/$NAME-$VERSION.tar.gz" "./$NAME-$VERSION"
cd ..
cp ./publish/tar/* ~/rpmbuild/SOURCES/
rpmbuild -bb nettag.spec
mv ~/rpmbuild/RPMS/x86_64/* ./publish/rpm/