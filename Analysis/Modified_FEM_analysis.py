######## Script to create the Distribution of Synthetic Covert Cues ########

import numpy as np
import matplotlib.pyplot as plt

# Define parameters
num_values = 10057
mean1 = 0.6
std_dev1 = 0.15
mean2 = 0.9
std_dev2 = 0.15

# Generate values from the skewed distribution
values1 = np.random.normal(loc=mean1, scale=std_dev1, size=int(num_values * 0.8))  # 80% of values
values2 = np.random.normal(loc=mean2, scale=std_dev2, size=int(num_values * 0.2))  # 20% of values
values = np.concatenate((values1, values2))

# Ensure all values are within [0, 1]
values = [max(0, min(v, 1)) for v in values]

# Define the output file path
output_file = r"/GazeProximity_Covert.txt"

# Write the values to the output file
with open(output_file, 'w') as file:
    for value in values:
        file.write(str(value) + '\n')

print("Values have been written to:", output_file)

# Plot the distribution
plt.hist(values, bins=20, density=True, alpha=0.6, color='g', edgecolor='black')

# Set labels and title
plt.xlabel('Normalized Gaze Proximity')
plt.ylabel('Frequency')
plt.title('Distribution of Synthetic Covert Cues')

# Show plot
plt.show()

######## Script to create the Distribution of Synthetic Overt Cues ########


# Define parameters
num_values = 10057
mean1 = 1.0
std_dev1 = 0.1
mean2 = 0.8
std_dev2 = 0.15

# Generate values from the skewed distribution
values1 = np.random.normal(loc=mean1, scale=std_dev1, size=int(num_values * 0.8))  # 80% of values
values2 = np.random.normal(loc=mean2, scale=std_dev2, size=int(num_values * 0.2))  # 20% of values
values = np.concatenate((values1, values2))

# Ensure all values are within [0, 1]
values = [max(0, min(v, 1)) for v in values]

# Define the output file path
output_file = r"/GazeProximity_Overt.txt"

# Write the values to the output file
with open(output_file, 'w') as file:
    for value in values:
        file.write(str(value) + '\n')

print("Values have been written to:", output_file)

# Plot the distribution
plt.hist(values, bins=20, density=True, alpha=0.6, color='g', edgecolor='black')

# Set labels and title
plt.xlabel('Normalized Gaze Proximity')
plt.ylabel('Frequency')
plt.title('Distribution of Synthetic Overt Cues')

# Show plot
plt.show()

# Combining the data into one single input file

with open('GazeProximity_Overt.txt', 'r') as file1, open('GazeProximity_Covert.txt', 'r') as file2:
    # Read all lines from each file
    lines1 = file1.readlines()
    lines2 = file2.readlines()


combined_lines = []
for line1, line2 in zip(lines1, lines2):
    combined_lines.append(line1.strip())
    combined_lines.append(line2.strip())


with open('GazeProximity.txt', 'w') as combined_file:
    combined_file.write('\n'.join(combined_lines))

print("Files combined successfully.")

########################## 
# Create Cue Type TXT File 
###########################
def write_alternating_values(filename, num_lines):
  """
  Writes alternating values of 1.0 and 0.5 to a text file.

  Args:
    filename: The name of the output file.
    num_lines: The number of lines to write.
  """
  with open(filename, 'w') as f:
    for _ in range(num_lines):
      value = 0.5 if _ % 2 == 0 else 1.0
      f.write(f"{value}\n")

if __name__ == "__main__":
  # Set filename and number of lines
  filename = "CueType_2.txt"
  num_lines = 20400

  write_alternating_values(filename, num_lines)
  print(f"Successfully wrote alternating values to {filename}.")

##########################
# OVERT/COVERT/NO DIFFERENCE OVERRIDEN CUES ANALYSIS 
##########################

# This t-test analysis is done for all simulation settings for all the declared weights
from scipy import stats

def cohens_d(mean1, std_dev1, mean2, std_dev2):
    # Calculate pooled standard deviation
    pooled_std_dev = ((std_dev1 ** 2 + std_dev2 ** 2) / 2) ** 0.5
    
    # Calculate Cohen's d
    d = (mean1 - mean2) / pooled_std_dev
    
    return d

def paired_t_test_from_stats(mean1, std_dev1, n1, mean2, std_dev2, n2):
    # Calculate standard error of the difference
    std_error_diff = ((std_dev1**2 / n1) + (std_dev2**2 / n2))**0.5
    
    # Calculate t-statistic
    t_statistic = (mean1 - mean2) / std_error_diff
    
    # Calculate degrees of freedom
    dof = n1 + n2 - 2
    
    # Calculate p-value
    p_value = stats.t.sf(abs(t_statistic), dof) * 2
    
    # Print results
    print("T-Statistic:", t_statistic)
    print("P-Value:", p_value)
    
    # Interpret p-value
    alpha = 0.05
    if p_value < alpha:
        print("Reject null hypothesis: There is a significant difference between the means of the paired samples.")
    else:
        print("Fail to reject null hypothesis: There is no significant difference between the means of the paired samples.")

if __name__ == "__main__":
    mean1 = 0.427771277
    std_dev1 = 0.218152193
    n1 = 20401
    
    mean2 = 0.433357537
    std_dev2 = 0.235596455
    n2 = 20401
    
    # Perform paired t-test
    paired_t_test_from_stats(mean1, std_dev1, n1, mean2, std_dev2, n2)
    
    cohen_d = cohens_d(mean1, std_dev1, mean2, std_dev2)
    print("Cohen's d:", cohen_d)


