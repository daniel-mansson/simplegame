# tools/fastlane/lib/build_number.rb
#
# Build number management following the convention in Docs/BUILD_NUMBERS.md.
#
# Encoding scheme: aabbccdd (8 digits conceptually, stored as integer)
#   aa = major version component (0-99)
#   bb = minor version component (0-99)
#   cc = patch version component (0-99)
#   dd = build counter for this version (01-99)
#
# Example: version 0.1.0, counter 1 → 00_01_00_01 → 10001 (leading zeros dropped)
#
# Source of truth: ProjectSettings/ProjectSettings.asset
#   - bundleVersion: "a.b.c"
#   - AndroidBundleVersionCode: <integer>
#   - buildNumber.iPhone: <integer>

module BuildNumber
  # Decode an integer bundle number into its components.
  #
  # @param bundle_number [Integer] encoded bundle number
  # @return [Hash] { major:, minor:, patch:, counter: }
  def self.decode(bundle_number)
    n = bundle_number.to_i
    counter = n % 100
    patch   = (n / 100)  % 100
    minor   = (n / 10_000) % 100
    major   = (n / 1_000_000) % 100
    { major: major, minor: minor, patch: patch, counter: counter }
  end

  # Encode version components into an integer bundle number.
  #
  # @param major [Integer]
  # @param minor [Integer]
  # @param patch [Integer]
  # @param counter [Integer] build counter (1-99)
  # @return [Integer]
  def self.encode(major, minor, patch, counter)
    raise ArgumentError, "counter must be 1-99, got #{counter}" unless (1..99).include?(counter)
    raise ArgumentError, "major must be 0-99, got #{major}"     unless (0..99).include?(major)
    raise ArgumentError, "minor must be 0-99, got #{minor}"     unless (0..99).include?(minor)
    raise ArgumentError, "patch must be 0-99, got #{patch}"     unless (0..99).include?(patch)

    major * 1_000_000 + minor * 10_000 + patch * 100 + counter
  end

  # Parse the version string "a.b.c" into [major, minor, patch].
  #
  # @param version_string [String] e.g. "0.1.0"
  # @return [Array<Integer>]
  def self.parse_version(version_string)
    parts = version_string.strip.split(".")
    raise ArgumentError, "Invalid version format: #{version_string}" unless parts.length == 3

    parts.map do |p|
      Integer(p)
    rescue ArgumentError
      raise ArgumentError, "Invalid version component '#{p}' in #{version_string}"
    end
  end

  # Read the current bundle number from ProjectSettings.asset.
  #
  # @param project_root [String] path to project root (default: Dir.pwd)
  # @return [Integer] current AndroidBundleVersionCode
  def self.current(project_root: Dir.pwd)
    settings = read_project_settings(project_root)
    extract_android_build_number(settings)
  end

  # Read current version string from ProjectSettings.asset.
  #
  # @param project_root [String]
  # @return [String] e.g. "0.1.0"
  def self.current_version(project_root: Dir.pwd)
    settings = read_project_settings(project_root)
    extract_bundle_version(settings)
  end

  # Compute the next bundle number by incrementing the build counter (dd).
  # Resets counter to 1 if it would overflow 99.
  #
  # @param project_root [String]
  # @return [Integer] next bundle number
  def self.next_build(project_root: Dir.pwd)
    current_n = current(project_root: project_root)
    components = decode(current_n)
    next_counter = components[:counter] + 1
    next_counter = 1 if next_counter > 99

    encode(components[:major], components[:minor], components[:patch], next_counter)
  end

  # Write a new bundle number to ProjectSettings.asset.
  # Updates both AndroidBundleVersionCode and buildNumber.iPhone.
  #
  # @param project_root [String]
  # @param bundle_number [Integer]
  def self.write(project_root: Dir.pwd, bundle_number:)
    path = settings_path(project_root)
    content = File.read(path)

    # Update AndroidBundleVersionCode
    unless content.match?(/^\s*AndroidBundleVersionCode: \d+/)
      raise "Could not find AndroidBundleVersionCode in #{path}"
    end
    content = content.gsub(/^(\s*AndroidBundleVersionCode: )\d+/, "\\1#{bundle_number}")

    # Update buildNumber.iPhone (inside buildNumber block)
    # The block looks like:
    #   buildNumber:
    #     Standalone: 0
    #     iPhone: 12345
    content = content.gsub(/^(\s*iPhone: )\d+/, "\\1#{bundle_number}")

    File.write(path, content)
    puts "BuildNumber: wrote #{bundle_number} to #{path}"
  end

  # Human-readable summary of current build state.
  #
  # @param project_root [String]
  # @return [Hash]
  def self.summary(project_root: Dir.pwd)
    settings = read_project_settings(project_root)
    current_n = extract_android_build_number(settings)
    version   = extract_bundle_version(settings)
    components = decode(current_n)

    {
      version: version,
      bundle_number: current_n,
      major: components[:major],
      minor: components[:minor],
      patch: components[:patch],
      counter: components[:counter],
      next_bundle_number: next_build(project_root: project_root)
    }
  end

  # --- private helpers ---

  def self.settings_path(project_root)
    File.join(project_root, "ProjectSettings", "ProjectSettings.asset")
  end

  def self.read_project_settings(project_root)
    path = settings_path(project_root)
    raise "ProjectSettings.asset not found at #{path}" unless File.exist?(path)

    File.read(path)
  end

  def self.extract_android_build_number(content)
    match = content.match(/^\s*AndroidBundleVersionCode: (\d+)/)
    raise "Could not parse AndroidBundleVersionCode from ProjectSettings.asset" unless match

    match[1].to_i
  end

  def self.extract_bundle_version(content)
    match = content.match(/^\s*bundleVersion: (\S+)/)
    raise "Could not parse bundleVersion from ProjectSettings.asset" unless match

    match[1]
  end

  private_class_method :settings_path, :read_project_settings,
                       :extract_android_build_number, :extract_bundle_version
end
