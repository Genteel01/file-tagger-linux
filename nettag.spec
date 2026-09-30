Name:       nettag
Version:    0.11
Release:    2
Summary:    Software for editing metadata tags on audio files
License:    FIXME

Source0:    https://www.example.com/%{name}/releases/%{name}-%{version}.tar.gz
ExclusiveArch: x86_64

%undefine _debugsource_packages

%description
Software for editing metadata tags on audio files

%prep
%setup -q

%build

%install
mkdir -p %{buildroot}/%{_bindir}
mkdir -p %{buildroot}/%{_iconsdir}
mkdir -p %{buildroot}/%{_datadir}/applications
install -m 0755 %{name} %{buildroot}/%{_bindir}/%{name}
cp -r icons/* %{buildroot}/%{_iconsdir}/
cp -r %{name}.desktop %{buildroot}/%{_datadir}/applications/%{name}.desktop

%files
%{_bindir}/%{name}
%{_iconsdir}/hicolor/scalable/apps/nettag.svg
%{_datadir}/applications/%{name}.desktop

%changelog
* Wed Sep 30 2026 George Shepherd <georgeshepherd3@gmail.com> - 0.1-1
- First nettag package creation