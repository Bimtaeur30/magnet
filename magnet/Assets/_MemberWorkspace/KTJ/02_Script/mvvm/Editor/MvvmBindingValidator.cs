using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mvvm.Editor
{
    public static class MvvmBindingValidator
    {
        public static string Validate(MvvmBindingProfile profile)
        {
            if (profile == null)
            {
                return "No profile selected.";
            }

            var issues = new List<string>();

            if (profile.targetPrefab == null)
            {
                issues.Add("Target is not assigned.");
            }

            if (string.IsNullOrWhiteSpace(profile.viewClassName))
            {
                issues.Add("View class name is empty.");
            }

            if (string.IsNullOrWhiteSpace(profile.viewModelClassName))
            {
                issues.Add("ViewModel class name is empty.");
            }

            foreach (var binding in profile.bindings.Where(x => x.enabled))
            {
                if (string.IsNullOrWhiteSpace(binding.viewModelMember))
                {
                    issues.Add($"{binding.objectPath}: ViewModel member is empty.");
                }

                if (string.IsNullOrWhiteSpace(binding.fieldName))
                {
                    issues.Add($"{binding.objectPath}: generated field name is empty.");
                }
            }

            var duplicateMembers = profile.bindings
                .Where(x => x.enabled)
                .GroupBy(x => x.viewModelMember)
                .Where(x => !string.IsNullOrWhiteSpace(x.Key) && x.Select(y => y.valueType).Distinct().Count() > 1);

            foreach (var member in duplicateMembers)
            {
                issues.Add($"{member.Key}: same ViewModel member is used with different value types.");
            }

            if (issues.Count == 0)
            {
                return "No validation issues found.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("Validation issues:");
            foreach (var issue in issues)
            {
                sb.AppendLine("- " + issue);
            }

            return sb.ToString();
        }
    }
}
