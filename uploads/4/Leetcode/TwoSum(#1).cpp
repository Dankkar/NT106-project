class Solution {
public:
    vector<int> twoSum(vector<int>& nums, int target) {
        int complement = 0;
        int temp = 0;
        vector<int> ans;
        for (int i = 0; i < nums.size() - 1; i++) {
            for (int j = i + 1; j < nums.size(); j++) {
                temp = nums[i];
                temp += nums[j];
                if (temp != target) {
                    continue;
                } else {
                    complement = j;
                    ans.push_back(j);
                    ans.push_back(i);
                    return ans;
                }
            }
        }
        return ans;
    }
};