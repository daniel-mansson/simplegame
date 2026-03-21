# tools/fastlane/lib/dry_run.rb
#
# Shared dry-run wrapper for all Fastlane lanes.
#
# Usage in a lane:
#
#   with_dry_run(dry_run: options[:dry_run], name: "produce", description: "Register app on App Store Connect") do
#     produce(app_identifier: "com.example.app", app_name: "My App")
#   end
#
# In dry-run mode: logs intent, skips the block, returns mock_return.
# In live mode: executes the block and returns its result.
# Validation (config checks, env var presence) still runs in dry-run mode.

module DryRun
  # Wrap an action with dry-run support.
  #
  # @param dry_run [Boolean] true to skip execution and log intent
  # @param name [String] short action name for logging (e.g. "produce", "match")
  # @param description [String] human-readable description of what would happen
  # @param mock_return [Object] value to return in dry-run mode (default: nil)
  # @yield block to execute in live mode
  # @return [Object] block result in live mode, mock_return in dry-run mode
  def self.with_dry_run(dry_run:, name:, description:, mock_return: nil, &block)
    if dry_run
      puts "[DRY RUN] Would: #{description}"
      mock_return
    else
      puts "[LIVE] #{name}: #{description}"
      block.call
    end
  end

  # Check required environment variables. Raises with a clear message if missing.
  # Runs in both live and dry-run mode — config errors should surface early.
  #
  # @param vars [Array<String>] env var names that must be set
  # @param context [String] which lane is checking (for error messages)
  def self.require_env!(vars, context:)
    missing = vars.reject { |v| ENV[v] && !ENV[v].empty? }
    unless missing.empty?
      raise "#{context}: Missing required environment variables: #{missing.join(', ')}\n" \
            "Set them in your shell or in a .env.local file (gitignored)."
    end
  end

  # Optionally warn about missing env vars without raising.
  # Used for vars that are only needed in live mode.
  #
  # @param vars [Array<String>] env var names to check
  # @param context [String] which lane is checking
  # @param dry_run [Boolean] if true, only warn; if false, raise
  def self.require_env_or_warn!(vars, context:, dry_run: false)
    missing = vars.reject { |v| ENV[v] && !ENV[v].empty? }
    return if missing.empty?

    message = "#{context}: Missing environment variables: #{missing.join(', ')}"
    if dry_run
      puts "[DRY RUN] Warning: #{message} (not required for dry-run)"
    else
      raise message
    end
  end
end
