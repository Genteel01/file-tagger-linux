Name:       nettag
Version:    0.10.2
Release:    2
Summary:    Software for editing metadata tags on audio files
License:    FIXME

URL:        https://github.com/Genteel01/file-tagger-linux
Source0:    https://github.com/Genteel01/file-tagger-linux/releases/download/v%{version}/%{name}-%{version}.tar.gz
ExclusiveArch: x86_64

#Was failing to build due to debug list being empty
%undefine _debugsource_packages
# Fixes broken builds when build in github action, local builds were always fine
%global __strip /bin/true

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

* Thur Oct 1 2026 George Shepherd <georgeshepherd3@gmail.com> - 0.10.2-2
- Added URL and proper Source0 to spec