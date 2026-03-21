# tools/fastlane/lib/config.rb
#
# Config file loader for fastlane/config/*.json files.
# Provides validated access to app configuration without hardcoding values.
#
# Usage:
#   config = Config.load(path: "fastlane/config/app.json")
#   bundle_id = config["ios_bundle_id"]

require "json"

module Config
  # Load and parse a JSON config file.
  #
  # @param path [String] path to the JSON file (relative to project root or absolute)
  # @param required_keys [Array<String>] keys that must be present (raises if missing)
  # @return [Hash] parsed config
  def self.load(path:, required_keys: [])
    abs_path = File.expand_path(path)

    unless File.exist?(abs_path)
      raise "Config file not found: #{abs_path}\n" \
            "Check that fastlane/config/ files are present and the path is correct."
    end

    raw = File.read(abs_path)

    begin
      config = JSON.parse(raw)
    rescue JSON::ParserError => e
      raise "Config file is not valid JSON: #{abs_path}\n#{e.message}"
    end

    unless config.is_a?(Hash)
      raise "Config file must be a JSON object (Hash), got #{config.class}: #{abs_path}"
    end

    missing = required_keys.reject { |k| config.key?(k) }
    unless missing.empty?
      raise "Config file #{abs_path} is missing required keys: #{missing.join(', ')}"
    end

    config
  end

  # Load app config from fastlane/config/app.json.
  # Resolves env var references for secret fields.
  #
  # @param project_root [String]
  # @return [Hash] config with resolved env var names
  def self.load_app(project_root: Dir.pwd)
    path = File.join(project_root, "fastlane", "config", "app.json")
    load(path: path, required_keys: %w[
      ios_bundle_id
      android_package_name
      app_name
      primary_language
    ])
  end

  # Load build config from fastlane/config/build.json.
  #
  # @param project_root [String]
  # @return [Hash]
  def self.load_build(project_root: Dir.pwd)
    path = File.join(project_root, "fastlane", "config", "build.json")
    load(path: path, required_keys: %w[
      unity_path_env
      ios_output_dir
      android_output_dir
    ])
  end

  # Load tester config from fastlane/config/testers.json.
  #
  # @param project_root [String]
  # @return [Hash]
  def self.load_testers(project_root: Dir.pwd)
    path = File.join(project_root, "fastlane", "config", "testers.json")
    load(path: path, required_keys: %w[groups])
  end

  # Resolve an env var name from config to its actual value.
  # Raises if the env var is not set (in live mode) or warns (in dry-run mode).
  #
  # @param config [Hash] loaded config hash
  # @param key [String] config key whose value is an env var name
  # @param dry_run [Boolean]
  # @return [String, nil]
  def self.resolve_env(config, key, dry_run: false)
    env_var_name = config[key]
    return nil unless env_var_name

    value = ENV[env_var_name]
    if value.nil? || value.empty?
      message = "Environment variable '#{env_var_name}' (referenced by config key '#{key}') is not set"
      if dry_run
        puts "[DRY RUN] Warning: #{message}"
        nil
      else
        raise message
      end
    else
      value
    end
  end
end
