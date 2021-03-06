# encoding: utf-8
require 'rubygems'
require 'albacore'
require 'rake/clean'
require 'rexml/document'
require './customassemblyinfo'

OUTPUT = "build"
CONFIGURATION = 'Release'
CONFIGURATIONMONO = 'MonoRelease'
SHARED_ASSEMBLY_INFO = 'src/SharedAssemblyInfo.cs'
SOLUTION_FILE = 'src/Mesmer.sln'

Albacore.configure do |config|
    config.log_level = :verbose
    config.msbuild.use :net4
end

desc "Compiles solution and runs unit tests"
task :default => [:clean, :assembly_info, :compile, :test, :publish, :package]

desc "Executes all nunit tests"
task :test => [:nunit]

desc "Compiles solution and runs unit tests for Mono"
task :mono => [:clean, :assembly_info, :compilemono, :testmono]

desc "Executes all tests with Mono"
task :testmono => [:nunitmono]

#Add the folders that should be cleaned as part of the clean task
CLEAN.include(OUTPUT)
CLEAN.include(FileList["src/**/#{CONFIGURATION}"])

desc "Update shared assemblyinfo file for the build"
#assemblyinfo :assembly_info => [:clean] do |asm|
#    asm.company_name = "Mesmer"
#    asm.product_name = "Mesmer"
#    asm.title = "Mesmer"
#    asm.description = "A Memoization Libaray for the .NET platform"
#    asm.copyright = "Blair Davidson"
#    asm.output_file = SHARED_ASSEMBLY_INFO
#end
task :assembly_info do
  puts "Main project does not update assembly info"
end

desc "Compile solution file"
msbuild :compile => [:assembly_info] do |msb|
    msb.properties = { :configuration => CONFIGURATION, "VisualStudioVersion" => ENV['VS110COMNTOOLS'] ? "11.0" : "10.0" }
    msb.targets :Clean, :Build
    msb.solution = SOLUTION_FILE
end

desc "Compile solution file for Mono"
xbuild :compilemono => [:assembly_info] do |xb|
    xb.solution = SOLUTION_FILE
    xb.properties = { :configuration => CONFIGURATIONMONO, "TargetFrameworkProfile" => "", "TargetFrameworkVersion" => "v4.0" }
end

desc "Gathers output files and copies them to the output folder"
task :publish => [:compile] do

    output = "#{OUTPUT}/binaries"
    if not File.directory? output
        mkpath(output)
    end

    FileUtils.cp_r FileList["src/**/#{CONFIGURATION}/*.dll", "src/**/#{CONFIGURATION}/*.XML", "src/**/#{CONFIGURATION}/*.pdb", "src/**/*.ps1"].exclude(/obj\//).exclude(/.Tests/), output
end

desc "Executes nunit tests"
nunit :nunit => [:compile] do |nunit|
    tests = FileList["src/**/#{CONFIGURATION}/*.Tests*.dll"].exclude(/obj\//).exclude(/Mesmer.ViewEngines.Razor.Tests.Models/)

    nunit.command = "tools/nunit/bin/nunit-console-x86.exe"
    nunit.assemblies = tests
end 

desc "Executes nunit tests using Mono"
nunit :nunitmono => [] do |nunit|
    tests = FileList["src/**/#{CONFIGURATIONMONO}/*.Tests*.dll"].exclude(/obj\//).exclude(/Mesmer.ViewEngines.Razor.Tests.Models/)

    nunit.command = "tools/nunit/nunitmono.sh"
    nunit.assemblies = tests
end

desc "Zips up the built binaries for easy distribution"
zip :package => [:publish] do |zip|
    if not File.directory? "#{OUTPUT}/packages"
        Dir.mkdir("#{OUTPUT}/packages")
    end

    zip.directories_to_zip "#{OUTPUT}/binaries"
    zip.output_file = "Mesmer-Latest.zip"
    zip.output_path = "#{OUTPUT}/packages"
end

desc "Deletes symbol packages"
task :nuget_nuke_symbol_packages do
  nupkgs = FileList['**/*.Symbols.nupkg']
  nupkgs.each do |nupkg|
    puts "Deleting #{nupkg}"
    FileUtils.rm(nupkg)
  end
end

desc "Generates NuGet packages for each project that contains a nuspec"
task :nuget_package => [:publish] do
    if not File.directory? "#{OUTPUT}/nuget"
        Dir.mkdir("#{OUTPUT}/nuget")
    end
    nuspecs = FileList["src/**/Mesmer*.nuspec"]
    root = File.dirname(__FILE__)

    # Copy all project *.nuspec to nuget build folder before editing
    FileUtils.cp_r nuspecs, "#{OUTPUT}/nuget"
    nuspecs = FileList["#{OUTPUT}/nuget/Mesmer*.nuspec"]

    # Update the copied *.nuspec files to correct version numbers and other common values
    nuspecs.each do |nuspec|
        puts "Updating #{nuspec}"
        update_xml nuspec do |xml|
            # Override the version number in the nuspec file with the one from this rake file (set above)
            xml.root.elements["metadata/version"].text = $Mesmer_version
			
			# Override the Mesmer dependencies to match this version
            Mesmer_dependencies = xml.root.elements["metadata/dependencies/dependency[contains(@id,'Mesmer')]"]
            Mesmer_dependencies.attributes["version"] = "#{$Mesmer_version}" unless Mesmer_dependencies.nil?

            # Override common values
            xml.root.elements["metadata/authors"].text = "Blair Davidson"
            xml.root.elements["metadata/summary"].text = "Mesmer is a Memoization Library for the .Net platform"
            xml.root.elements["metadata/licenseUrl"].text = "https://github.com/Mesmer/Mesmer/blob/master/license.txt"
            xml.root.elements["metadata/projectUrl"].text = "http://loosechainsaw.github.io/Mesmer"
        end
    end

    # Generate the NuGet packages from the newly edited nuspec fileiles
    nuspecs.each do |nuspec|        
        nuget = NuGetPack.new
        nuget.command = "tools/nuget/nuget.exe"
        nuget.nuspec = "\"" + root + '/' + nuspec + "\""
        nuget.output = "#{OUTPUT}/nuget"
        nuget.parameters = "-Symbols", "-BasePath \"#{root}\""     #using base_folder throws as there are two options that begin with b in nuget 1.4
        nuget.execute
    end
end

desc "Pushes the nuget packages in the nuget folder up to the nuget gallary and symbolsource.org. Also publishes the packages into the feeds."
task :nuget_publish, :api_key do |task, args|
    nupkgs = FileList["#{OUTPUT}/nuget/*#{$Mesmer_version}.nupkg"]
    nupkgs.each do |nupkg| 
        puts "Pushing #{nupkg}"
        nuget_push = NuGetPush.new
	nuget_push.apikey = args.api_key if !args.empty?
        nuget_push.command = "tools/nuget/nuget.exe"
        nuget_push.package = "\"" + nupkg + "\""
        nuget_push.create_only = false
        nuget_push.execute
    end
end

desc "Pushes the nuget packages in the nuget folder up to the specified feed"
task :nuget_publish_alt, :api_key, :source do |task, args|
    raise "Missing source" if args.source.nil?
    nupkgs = FileList["#{OUTPUT}/nuget/*#{$Mesmer_version}.nupkg"]
    nupkgs.each do |nupkg| 
        puts "Pushing #{nupkg} to {#args.source}"
        nuget_push = NuGetPush.new
        nuget_push.apikey = args.api_key if !args.empty?
        nuget_push.command = "tools/nuget/nuget.exe"
        nuget_push.package = "\"" + nupkg + "\""
        nuget_push.source = args.source
        nuget_push.create_only = false
        nuget_push.execute
    end
end

desc "Updates the SharedAssemblyInfo version"
assemblyinfo :update_version, [:version] do |asm, args|
    asm.input_file = SHARED_ASSEMBLY_INFO
    asm.version = args.version if !args.version.nil?
    asm.output_file = SHARED_ASSEMBLY_INFO
end

desc "Updates the SharedAssemblyInfo informational version"
customassemblyinfo :update_informational_version, [:version] do |asm, args|
    asm.input_file = SHARED_ASSEMBLY_INFO
    asm.assembly_informational_version = args.version if !args.version.nil?
    asm.output_file = SHARED_ASSEMBLY_INFO
end

desc "Tags the current release"
task :tag, :assembly_info do |asm, args|
    args.with_defaults(:assembly_info => $Mesmer_version)

    sh "git tag \"v#{args.version}\""
end

desc "Updates the version and tags the release"
task :prep_release, :assembly_info do |task, args|
  if !args.version.nil?
    task(:update_version).invoke(args.version)

    sh "git add #{SHARED_ASSEMBLY_INFO}"
    sh "git commit -m \"Updated version to #{args.version}\""

    task(:tag).invoke(args.version)
  end
end

def update_xml(xml_path)
    #Open up the xml file
    xml_file = File.new(xml_path)
    xml = REXML::Document.new xml_file
 
    #Allow caller to make the changes
    yield xml
 
    xml_file.close
         
    #Save the changes
    xml_file = File.open(xml_path, "w")
    formatter = REXML::Formatters::Default.new(5)
    formatter.write(xml, xml_file)
    xml_file.close 
end

def get_assembly_version(file)
  return '' if file.nil?

  File.open(file, 'r') do |file|
    file.each_line do |line|
      result = /\[assembly: AssemblyInformationalVersion\(\"(.*?)\"\)\]/.match(line)

      return result[1] if !result.nil?
    end
  end

  ''
end

$Mesmer_version = get_assembly_version SHARED_ASSEMBLY_INFO
puts "Version: #{$Mesmer_version}"

